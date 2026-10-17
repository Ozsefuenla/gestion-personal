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
        IRequestValidator requestValidator)
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

        return await _createHandler.Handle(command, cancellationToken);
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

        return await _changePinHandler.Handle(workerId, command, cancellationToken);
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

        return await _renameHandler.Handle(workerId, command, cancellationToken);
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

        return await _resetPinHandler.Handle(workerId, command, cancellationToken);
    }

    public Task<Result> ActivateAsync(int workerId, CancellationToken cancellationToken = default) =>
        _activateHandler.Handle(workerId, cancellationToken);

    public Task<Result> DeactivateAsync(int workerId, CancellationToken cancellationToken = default) =>
        _deactivateHandler.Handle(workerId, cancellationToken);
}
