using GestionPersonal.Application.Abstractions;
using GestionPersonal.Application.Common;

namespace GestionPersonal.Application.TimeEntries;

public sealed class QuickClockCommandHandler
{
    private readonly IWorkerRepository _workerRepository;
    private readonly ITimeEntryRepository _timeEntryRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ICurrentCenter _currentCenter;
    private readonly ILoginAttemptTracker _loginAttemptTracker;
    private readonly StartTimeEntryCommandHandler _startHandler;
    private readonly PauseTimeEntryCommandHandler _pauseHandler;
    private readonly ResumeTimeEntryCommandHandler _resumeHandler;
    private readonly EndTimeEntryCommandHandler _endHandler;

    public QuickClockCommandHandler(
        IWorkerRepository workerRepository,
        ITimeEntryRepository timeEntryRepository,
        IPasswordHasher passwordHasher,
        ICurrentCenter currentCenter,
        ILoginAttemptTracker loginAttemptTracker,
        StartTimeEntryCommandHandler startHandler,
        PauseTimeEntryCommandHandler pauseHandler,
        ResumeTimeEntryCommandHandler resumeHandler,
        EndTimeEntryCommandHandler endHandler)
    {
        _workerRepository = workerRepository;
        _timeEntryRepository = timeEntryRepository;
        _passwordHasher = passwordHasher;
        _currentCenter = currentCenter;
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
        var centerId = _currentCenter.CenterId;

        if (centerId is null)
        {
            return Error.Forbidden("No autenticado.");
        }

        if (_loginAttemptTracker.IsLockedOut(command.WorkerId))
        {
            return Error.Forbidden("Demasiados intentos fallidos. Inténtalo de nuevo más tarde.");
        }

        var worker = await _workerRepository.GetByIdAndCenterAsync(command.WorkerId, centerId.Value, cancellationToken);

        if (worker is null)
        {
            return Error.NotFound("El trabajador no existe.");
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
            "start" when currentStatus == "paused" => await _resumeHandler.Handle(new ResumeTimeEntryCommand(worker.Id), cancellationToken),
            "start" => await _startHandler.Handle(new StartTimeEntryCommand(worker.Id), cancellationToken),
            "pause" => await _pauseHandler.Handle(new PauseTimeEntryCommand(worker.Id), cancellationToken),
            "end" => await _endHandler.Handle(new EndTimeEntryCommand(worker.Id), cancellationToken),
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
