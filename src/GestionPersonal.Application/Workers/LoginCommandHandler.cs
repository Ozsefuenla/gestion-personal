using GestionPersonal.Application.Abstractions;
using GestionPersonal.Application.Common;
using GestionPersonal.Application.TimeEntries;

namespace GestionPersonal.Application.Workers;

public sealed class LoginCommandHandler
{
    private readonly IWorkerRepository _workerRepository;
    private readonly ITimeEntryRepository _timeEntryRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ICurrentCenter _currentCenter;
    private readonly ILoginAttemptTracker _loginAttemptTracker;

    public LoginCommandHandler(
        IWorkerRepository workerRepository,
        ITimeEntryRepository timeEntryRepository,
        IPasswordHasher passwordHasher,
        ICurrentCenter currentCenter,
        ILoginAttemptTracker loginAttemptTracker)
    {
        _workerRepository = workerRepository;
        _timeEntryRepository = timeEntryRepository;
        _passwordHasher = passwordHasher;
        _currentCenter = currentCenter;
        _loginAttemptTracker = loginAttemptTracker;
    }

    public async Task<Result<WorkerStatusResponse>> Handle(
        LoginCommand command,
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

        return await WorkerStatusBuilder.BuildAsync(worker, now, from, to, _timeEntryRepository, cancellationToken);
    }
}
