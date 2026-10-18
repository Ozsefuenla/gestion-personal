using GestionPersonal.Application.Centers;
using GestionPersonal.Application.Common;
using GestionPersonal.Application.Platform;

namespace GestionPersonal.Application.Services;

public sealed class PlatformService : IPlatformService
{
    private readonly ListAllCentersQueryHandler _listCentersHandler;
    private readonly CreateCenterCommandHandler _createCenterHandler;
    private readonly SetCenterActiveCommandHandler _setCenterActiveHandler;
    private readonly CreateCenterAdminCommandHandler _createCenterAdminHandler;
    private readonly ListCenterUsersQueryHandler _listCenterUsersHandler;
    private readonly CreateCenterUserCommandHandler _createCenterUserHandler;
    private readonly SetCenterUserActiveCommandHandler _setCenterUserActiveHandler;
    private readonly ResetCenterUserPasswordCommandHandler _resetCenterUserPasswordHandler;
    private readonly IRequestValidator _requestValidator;

    public PlatformService(
        ListAllCentersQueryHandler listCentersHandler,
        CreateCenterCommandHandler createCenterHandler,
        SetCenterActiveCommandHandler setCenterActiveHandler,
        CreateCenterAdminCommandHandler createCenterAdminHandler,
        ListCenterUsersQueryHandler listCenterUsersHandler,
        CreateCenterUserCommandHandler createCenterUserHandler,
        SetCenterUserActiveCommandHandler setCenterUserActiveHandler,
        ResetCenterUserPasswordCommandHandler resetCenterUserPasswordHandler,
        IRequestValidator requestValidator)
    {
        _listCentersHandler = listCentersHandler;
        _createCenterHandler = createCenterHandler;
        _setCenterActiveHandler = setCenterActiveHandler;
        _createCenterAdminHandler = createCenterAdminHandler;
        _listCenterUsersHandler = listCenterUsersHandler;
        _createCenterUserHandler = createCenterUserHandler;
        _setCenterUserActiveHandler = setCenterUserActiveHandler;
        _resetCenterUserPasswordHandler = resetCenterUserPasswordHandler;
        _requestValidator = requestValidator;
    }

    public Task<Result<IReadOnlyList<CenterResponse>>> ListCentersAsync(CancellationToken cancellationToken = default) =>
        _listCentersHandler.Handle(cancellationToken);

    public async Task<Result<CenterResponse>> CreateCenterAsync(CreateCenterCommand command, CancellationToken cancellationToken = default)
    {
        var validationError = await _requestValidator.ValidateAsync(command, cancellationToken);

        return validationError ?? await _createCenterHandler.Handle(command, cancellationToken);
    }

    public Task<Result> SetCenterActiveAsync(SetCenterActiveCommand command, CancellationToken cancellationToken = default) =>
        _setCenterActiveHandler.Handle(command, cancellationToken);

    public async Task<Result<CenterUserResponse>> CreateCenterAdminAsync(CreateCenterAdminCommand command, CancellationToken cancellationToken = default)
    {
        var validationError = await _requestValidator.ValidateAsync(command, cancellationToken);

        return validationError ?? await _createCenterAdminHandler.Handle(command, cancellationToken);
    }

    public Task<Result<IReadOnlyList<CenterUserResponse>>> ListCenterUsersAsync(int centerId, CancellationToken cancellationToken = default) =>
        _listCenterUsersHandler.Handle(new ListCenterUsersQuery(centerId), cancellationToken);

    public async Task<Result<CenterUserResponse>> CreateCenterUserAsync(CreateCenterUserCommand command, CancellationToken cancellationToken = default)
    {
        var validationError = await _requestValidator.ValidateAsync(command, cancellationToken);

        return validationError ?? await _createCenterUserHandler.Handle(command, cancellationToken);
    }

    public Task<Result> SetCenterUserActiveAsync(SetCenterUserActiveCommand command, CancellationToken cancellationToken = default) =>
        _setCenterUserActiveHandler.Handle(command, cancellationToken);

    public async Task<Result> ResetCenterUserPasswordAsync(ResetCenterUserPasswordCommand command, CancellationToken cancellationToken = default)
    {
        var validationError = await _requestValidator.ValidateAsync(command, cancellationToken);

        return validationError ?? await _resetCenterUserPasswordHandler.Handle(command, cancellationToken);
    }
}
