using GestionPersonal.Application.Abstractions;
using GestionPersonal.Application.Common;

namespace GestionPersonal.Application.TimeEntries;

public sealed class QuickClockCommandHandler
{
    private readonly IWorkerRepository _workerRepository;
    private readonly ITimeEntryRepository _timeEntryRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ICurrentUser _currentUser;
    private readonly ILoginAttemptTracker _loginAttemptTracker;
    private readonly StartTimeEntryCommandHandler _startHandler;
    private readonly PauseTimeEntryCommandHandler _pauseHandler;
    private readonly ResumeTimeEntryCommandHandler _resumeHandler;
    private readonly EndTimeEntryCommandHandler _endHandler;

    public QuickClockCommandHandler(
        IWorkerRepository workerRepository,
        ITimeEntryRepository timeEntryRepository,
        IPasswordHasher passwordHasher,
        ICurrentUser currentUser,
        ILoginAttemptTracker loginAttemptTracker,
        StartTimeEntryCommandHandler startHandler,
        PauseTimeEntryCommandHandler pauseHandler,
        ResumeTimeEntryCommandHandler resumeHandler,
        EndTimeEntryCommandHandler endHandler)
    {
        _workerRepository = workerRepository;
        _timeEntryRepository = timeEntryRepository;
        _passwordHasher = passwordHasher;
        _currentUser = currentUser;
        _loginAttemptTracker = loginAttemptTracker;
        _startHandler = startHandler;
        _pauseHandler = pauseHandler;
        _resumeHandler = resumeHandler;
        _endHandler = endHandler;
    }

    public async Task<Result<QuickClockResponse>> Handle(
        QuickClockCommand command,
        CancellationToken cancellationToken = default)
    {
        if (AccessControl.RequireKiosk(_currentUser) is { } accessError)
        {
            return accessError;
        }

        if (_loginAttemptTracker.IsLockedOut(command.WorkerId))
        {
            return Error.Forbidden("Demasiados intentos fallidos. Inténtalo de nuevo más tarde.");
        }

        var worker = await _workerRepository.GetByIdAndCenterAsync(command.WorkerId, _currentUser.CenterId!.Value, cancellationToken);

        if (worker is null)
        {
            return Error.NotFound("El trabajador no existe.");
        }

        if (!worker.Active)
        {
            return Error.Forbidden("El trabajador está inactivo. Contacta con el administrador.");
        }

        if (!_passwordHasher.Verify(command.Pin, worker.PinHash))
        {
            _loginAttemptTracker.RecordFailure(command.WorkerId);
            return Error.Validation("El PIN introducido no es correcto.");
        }

        _loginAttemptTracker.Reset(command.WorkerId);

        var now = DateTimeOffset.UtcNow;
        var (from, to) = TodayStatusHelper.GetTodayRange(now);

        var openEntry = await _timeEntryRepository.GetOpenEntryByWorkerAsync(worker.Id, cancellationToken);
        var entries = await _timeEntryRepository.GetByWorkerBetweenAsync(worker.Id, from, to, cancellationToken);

        var currentStatus = TodayStatusHelper.GetCurrentStatus(openEntry, entries);
        var availableActions = GetAvailableActions(currentStatus);
        var staleEntryDate = TodayStatusHelper.GetStaleEntryDate(openEntry, now);

        if (command.Action == "end-at")
        {
            if (command.EndDate is null || command.EndTime is null)
            {
                return Error.Validation("Indica la hora de finalización.");
            }

            var endResult = await _endHandler.EndForWorkerAtAsync(
                worker.Id,
                TimeStamp.Compose(command.EndDate.Value, command.EndTime.Value),
                cancellationToken);

            if (endResult.IsFailure)
            {
                return endResult.Error;
            }

            return new QuickClockResponse(true, "La jornada anterior se ha finalizado.", null, []);
        }

        if (command.Action == "start" && staleEntryDate is not null)
        {
            return new QuickClockResponse(
                false,
                $"La jornada del día {staleEntryDate:dd/MM/yyyy} no se finalizó. Indica la hora de finalización.",
                currentStatus,
                [],
                staleEntryDate);
        }

        if (!availableActions.Contains(command.Action))
        {
            return new QuickClockResponse(
                false,
                BuildMessage(currentStatus, availableActions),
                currentStatus,
                availableActions);
        }

        var result = command.Action switch
        {
            "start" when currentStatus == "paused" => await _resumeHandler.ResumeForWorkerAsync(worker.Id, cancellationToken),
            "start" => await _startHandler.StartForWorkerAsync(worker.Id, cancellationToken),
            "pause" => await _pauseHandler.PauseForWorkerAsync(worker.Id, cancellationToken),
            "end" => await _endHandler.EndForWorkerAsync(worker.Id, cancellationToken),
            _ => throw new InvalidOperationException("Acción no soportada.")
        };

        if (result.IsFailure)
        {
            return result.Error;
        }

        return new QuickClockResponse(true, "Fichaje realizado correctamente.", null, []);
    }

    private static IReadOnlyList<string> GetAvailableActions(string currentStatus) => currentStatus switch
    {
        "idle" => ["start"],
        "in-progress" => ["pause", "end"],
        "paused" => ["start", "end"],
        _ => []
    };

    private static string BuildMessage(string currentStatus, IReadOnlyList<string> actions)
    {
        if (currentStatus == "finished")
        {
            return "Tu jornada ya está finalizada. Si tienes que hacer modificaciones, ve a tu espacio personal.";
        }

        var label = currentStatus switch
        {
            "idle" => "Sin iniciar",
            "in-progress" => "En curso",
            "paused" => "En pausa",
            "finished" => "Finalizado",
            _ => currentStatus
        };

        if (actions.Count == 0)
        {
            return $"Tu estado actual es '{label}'. No hay acciones disponibles.";
        }

        var actionList = string.Join(", ", actions.Select(action => action switch
        {
            "start" => "Iniciar",
            "pause" => "Pausar",
            "end" => "Finalizar",
            _ => action
        }));

        return $"Tu estado actual es '{label}'. Puedes: {actionList}.";
    }
}
