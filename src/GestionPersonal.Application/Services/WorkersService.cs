using GestionPersonal.Application.Abstractions;
using GestionPersonal.Application.Common;
using GestionPersonal.Application.Workers;

namespace GestionPersonal.Application.Services;

public sealed class WorkersService : IWorkersService
{
    private readonly ListWorkersQueryHandler _listActiveHandler;
    private readonly ListWorkersByCenterQueryHandler _listByCenterHandler;
    private readonly ListWorkersForAdminQueryHandler _listAllHandler;
    private readonly GetWorkersStatusQueryHandler _getStatusesHandler;
    private readonly GetWorkerStatusQueryHandler _getStatusHandler;
    private readonly LoginCommandHandler _loginHandler;
    private readonly CreateWorkerCommandHandler _createHandler;
    private readonly ChangePinCommandHandler _changePinHandler;
    private readonly RenameWorkerCommandHandler _renameHandler;
    private readonly ResetWorkerPinCommandHandler _resetPinHandler;
    private readonly ActivateWorkerCommandHandler _activateHandler;
    private readonly DeactivateWorkerCommandHandler _deactivateHandler;
    private readonly IRequestValidator _requestValidator;
    private readonly IEventCollector _events;
    private readonly ICurrentUser _currentUser;

    public WorkersService(
        ListWorkersQueryHandler listActiveHandler,
        ListWorkersByCenterQueryHandler listByCenterHandler,
        ListWorkersForAdminQueryHandler listAllHandler,
        GetWorkersStatusQueryHandler getStatusesHandler,
        GetWorkerStatusQueryHandler getStatusHandler,
        LoginCommandHandler loginHandler,
        CreateWorkerCommandHandler createHandler,
        ChangePinCommandHandler changePinHandler,
        RenameWorkerCommandHandler renameHandler,
        ResetWorkerPinCommandHandler resetPinHandler,
        ActivateWorkerCommandHandler activateHandler,
        DeactivateWorkerCommandHandler deactivateHandler,
        IRequestValidator requestValidator,
        IEventCollector events,
        ICurrentUser currentUser)
    {
        _listActiveHandler = listActiveHandler;
        _listByCenterHandler = listByCenterHandler;
        _listAllHandler = listAllHandler;
        _getStatusesHandler = getStatusesHandler;
        _getStatusHandler = getStatusHandler;
        _loginHandler = loginHandler;
        _createHandler = createHandler;
        _changePinHandler = changePinHandler;
        _renameHandler = renameHandler;
        _resetPinHandler = resetPinHandler;
        _activateHandler = activateHandler;
        _deactivateHandler = deactivateHandler;
        _requestValidator = requestValidator;
        _events = events;
        _currentUser = currentUser;
    }

    public Task<Result<IReadOnlyList<WorkerResponse>>> ListActiveAsync(CancellationToken cancellationToken = default) =>
        _listActiveHandler.Handle(new ListWorkersQuery(), cancellationToken);

    public Task<Result<IReadOnlyList<WorkerResponse>>> ListByCenterAsync(
        int centerId,
        CancellationToken cancellationToken = default) =>
        _listByCenterHandler.Handle(new ListWorkersByCenterQuery(centerId), cancellationToken);

    public Task<Result<IReadOnlyList<WorkerResponse>>> ListAllAsync(CancellationToken cancellationToken = default) =>
        _listAllHandler.Handle(new ListWorkersForAdminQuery(), cancellationToken);

    public Task<Result<IReadOnlyList<WorkerStatusResponse>>> GetStatusesAsync(
        CancellationToken cancellationToken = default) =>
        _getStatusesHandler.Handle(cancellationToken);

    public Task<Result<WorkerStatusResponse>> GetStatusAsync(
        int workerId,
        CancellationToken cancellationToken = default) =>
        _getStatusHandler.Handle(workerId, cancellationToken);

    public async Task<Result<WorkerStatusResponse>> LoginAsync(
        LoginCommand command,
        CancellationToken cancellationToken = default)
    {
        var validationError = await _requestValidator.ValidateAsync(command, cancellationToken);

        if (validationError is not null)
        {
            return validationError;
        }

        return await _loginHandler.Handle(command, cancellationToken);
    }

    public async Task<Result<WorkerResponse>> CreateAsync(
        CreateWorkerCommand command,
        CancellationToken cancellationToken = default)
    {
        var validationError = await _requestValidator.ValidateAsync(command, cancellationToken);

        if (validationError is not null)
        {
            return validationError;
        }

        var result = await _createHandler.Handle(command, cancellationToken);

        if (result.IsSuccess)
        {
            Emit(
                "business",
                "worker.created",
                $"Trabajador '{result.Value.FullName}' creado",
                command.CenterId,
                result.Value.Id);
        }

        return result;
    }

    public async Task<Result> ChangePinAsync(
        int workerId,
        ChangePinCommand command,
        CancellationToken cancellationToken = default)
    {
        var validationError = await _requestValidator.ValidateAsync(command, cancellationToken);

        if (validationError is not null)
        {
            return validationError;
        }

        var result = await _changePinHandler.Handle(workerId, command, cancellationToken);

        if (result.IsSuccess)
        {
            Emit("security", "worker.pin_changed", $"PIN del trabajador {workerId} cambiado", subjectId: workerId);
        }

        return result;
    }

    public async Task<Result> RenameAsync(
        int workerId,
        RenameWorkerCommand command,
        CancellationToken cancellationToken = default)
    {
        var validationError = await _requestValidator.ValidateAsync(command, cancellationToken);

        if (validationError is not null)
        {
            return validationError;
        }

        var result = await _renameHandler.Handle(workerId, command, cancellationToken);

        if (result.IsSuccess)
        {
            Emit("business", "worker.renamed", $"Trabajador {workerId} renombrado a '{command.FullName}'", subjectId: workerId);
        }

        return result;
    }

    public async Task<Result> ResetPinAsync(
        int workerId,
        ResetWorkerPinCommand command,
        CancellationToken cancellationToken = default)
    {
        var validationError = await _requestValidator.ValidateAsync(command, cancellationToken);

        if (validationError is not null)
        {
            return validationError;
        }

        var result = await _resetPinHandler.Handle(workerId, command, cancellationToken);

        if (result.IsSuccess)
        {
            Emit("security", "worker.pin_reset", $"PIN del trabajador {workerId} reseteado", subjectId: workerId);
        }

        return result;
    }

    public async Task<Result> ActivateAsync(int workerId, CancellationToken cancellationToken = default)
    {
        var result = await _activateHandler.Handle(workerId, cancellationToken);

        if (result.IsSuccess)
        {
            Emit("business", "worker.activated", $"Trabajador {workerId} reactivado", subjectId: workerId);
        }

        return result;
    }

    public async Task<Result> DeactivateAsync(int workerId, CancellationToken cancellationToken = default)
    {
        var result = await _deactivateHandler.Handle(workerId, cancellationToken);

        if (result.IsSuccess)
        {
            Emit("business", "worker.deactivated", $"Trabajador {workerId} dado de baja", subjectId: workerId);
        }

        return result;
    }

    private void Emit(
        string category,
        string eventType,
        string message,
        int? centerId = null,
        int? subjectId = null)
    {
        _events.Emit(new AppEvent(
            category,
            eventType,
            category == "security" ? "Warning" : "Info",
            message,
            ActorRole: _currentUser.Role,
            ActorId: _currentUser.SubjectId,
            CenterId: centerId ?? _currentUser.CenterId,
            Data: subjectId is null
                ? null
                : new Dictionary<string, object?> { ["subjectId"] = subjectId }));
    }
}
