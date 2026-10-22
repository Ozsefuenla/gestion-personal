using GestionPersonal.Application.Abstractions;
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
    private readonly IEventCollector _events;
    private readonly ICurrentUser _currentUser;

    public PlatformService(
        ListAllCentersQueryHandler listCentersHandler,
        CreateCenterCommandHandler createCenterHandler,
        SetCenterActiveCommandHandler setCenterActiveHandler,
        CreateCenterAdminCommandHandler createCenterAdminHandler,
        ListCenterUsersQueryHandler listCenterUsersHandler,
        CreateCenterUserCommandHandler createCenterUserHandler,
        SetCenterUserActiveCommandHandler setCenterUserActiveHandler,
        ResetCenterUserPasswordCommandHandler resetCenterUserPasswordHandler,
        IRequestValidator requestValidator,
        IEventCollector events,
        ICurrentUser currentUser)
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
        _events = events;
        _currentUser = currentUser;
    }

    public Task<Result<IReadOnlyList<CenterResponse>>> ListCentersAsync(CancellationToken cancellationToken = default) =>
        _listCentersHandler.Handle(cancellationToken);

    public async Task<Result<CenterResponse>> CreateCenterAsync(CreateCenterCommand command, CancellationToken cancellationToken = default)
    {
        var validationError = await _requestValidator.ValidateAsync(command, cancellationToken);

        if (validationError is not null)
        {
            return validationError;
        }

        var result = await _createCenterHandler.Handle(command, cancellationToken);

        if (result.IsSuccess)
        {
            Emit("business", "center.created", $"Centro '{result.Value.Name}' creado", result.Value.Id);
        }

        return result;
    }

    public async Task<Result> SetCenterActiveAsync(SetCenterActiveCommand command, CancellationToken cancellationToken = default)
    {
        var result = await _setCenterActiveHandler.Handle(command, cancellationToken);

        if (result.IsSuccess)
        {
            Emit(
                "business",
                command.Active ? "center.activated" : "center.deactivated",
                $"Centro {command.CenterId} {(command.Active ? "activado" : "desactivado")}",
                command.CenterId);
        }

        return result;
    }

    public async Task<Result<CenterUserResponse>> CreateCenterAdminAsync(CreateCenterAdminCommand command, CancellationToken cancellationToken = default)
    {
        var validationError = await _requestValidator.ValidateAsync(command, cancellationToken);

        if (validationError is not null)
        {
            return validationError;
        }

        var result = await _createCenterAdminHandler.Handle(command, cancellationToken);

        if (result.IsSuccess)
        {
            Emit("business", "center_user.created", $"Administrador '{result.Value.Username}' creado", command.CenterId, result.Value.Id);
        }

        return result;
    }

    public Task<Result<IReadOnlyList<CenterUserResponse>>> ListCenterUsersAsync(int centerId, CancellationToken cancellationToken = default) =>
        _listCenterUsersHandler.Handle(new ListCenterUsersQuery(centerId), cancellationToken);

    public async Task<Result<CenterUserResponse>> CreateCenterUserAsync(CreateCenterUserCommand command, CancellationToken cancellationToken = default)
    {
        var validationError = await _requestValidator.ValidateAsync(command, cancellationToken);

        if (validationError is not null)
        {
            return validationError;
        }

        var result = await _createCenterUserHandler.Handle(command, cancellationToken);

        if (result.IsSuccess)
        {
            Emit(
                "business",
                "center_user.created",
                $"Usuario de tablet '{result.Value.Username}' creado",
                result.Value.CenterId,
                result.Value.Id);
        }

        return result;
    }

    public async Task<Result> SetCenterUserActiveAsync(SetCenterUserActiveCommand command, CancellationToken cancellationToken = default)
    {
        var result = await _setCenterUserActiveHandler.Handle(command, cancellationToken);

        if (result.IsSuccess)
        {
            Emit(
                "business",
                command.Active ? "center_user.activated" : "center_user.deactivated",
                $"Usuario {command.UserId} {(command.Active ? "activado" : "desactivado")}",
                subjectId: command.UserId);
        }

        return result;
    }

    public async Task<Result> ResetCenterUserPasswordAsync(ResetCenterUserPasswordCommand command, CancellationToken cancellationToken = default)
    {
        var validationError = await _requestValidator.ValidateAsync(command, cancellationToken);

        if (validationError is not null)
        {
            return validationError;
        }

        var result = await _resetCenterUserPasswordHandler.Handle(command, cancellationToken);

        if (result.IsSuccess)
        {
            Emit(
                "security",
                "center_user.password_reset",
                $"Contraseña del usuario {command.UserId} reseteada",
                subjectId: command.UserId);
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
