using GestionPersonal.Application.Abstractions;
using GestionPersonal.Application.Common;
using GestionPersonal.Application.Vacations;

namespace GestionPersonal.Application.Services;

public sealed class VacationsService : IVacationsService
{
    private readonly ListVacationsQueryHandler _listHandler;
    private readonly ListCenterVacationsQueryHandler _listCenterHandler;
    private readonly RequestVacationCommandHandler _requestHandler;
    private readonly DeleteVacationCommandHandler _deleteHandler;
    private readonly ApproveVacationCommandHandler _approveHandler;
    private readonly RejectVacationCommandHandler _rejectHandler;
    private readonly IRequestValidator _requestValidator;
    private readonly IEventCollector _events;
    private readonly ICurrentUser _currentUser;

    public VacationsService(
        ListVacationsQueryHandler listHandler,
        ListCenterVacationsQueryHandler listCenterHandler,
        RequestVacationCommandHandler requestHandler,
        DeleteVacationCommandHandler deleteHandler,
        ApproveVacationCommandHandler approveHandler,
        RejectVacationCommandHandler rejectHandler,
        IRequestValidator requestValidator,
        IEventCollector events,
        ICurrentUser currentUser)
    {
        _listHandler = listHandler;
        _listCenterHandler = listCenterHandler;
        _requestHandler = requestHandler;
        _deleteHandler = deleteHandler;
        _approveHandler = approveHandler;
        _rejectHandler = rejectHandler;
        _requestValidator = requestValidator;
        _events = events;
        _currentUser = currentUser;
    }

    public Task<Result<IReadOnlyList<VacationResponse>>> ListAsync(
        ListVacationsQuery query,
        CancellationToken cancellationToken = default) =>
        _listHandler.Handle(query, cancellationToken);

    public Task<Result<IReadOnlyList<VacationResponse>>> ListCenterAsync(
        CancellationToken cancellationToken = default) =>
        _listCenterHandler.Handle(new ListCenterVacationsQuery(), cancellationToken);

    public async Task<Result<VacationResponse>> RequestAsync(
        RequestVacationCommand command,
        CancellationToken cancellationToken = default)
    {
        var validationError = await _requestValidator.ValidateAsync(command, cancellationToken);

        if (validationError is not null)
        {
            return validationError;
        }

        var result = await _requestHandler.Handle(command, cancellationToken);

        if (result.IsSuccess)
        {
            Emit("business", "vacation.requested", "Ausencia solicitada", result.Value.Id);
        }

        return result;
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _deleteHandler.Handle(id, cancellationToken);

        if (result.IsSuccess)
        {
            Emit("business", "vacation.deleted", $"Ausencia {id} eliminada", id);
        }

        return result;
    }

    public async Task<Result<VacationResponse>> ApproveAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _approveHandler.Handle(id, cancellationToken);

        if (result.IsSuccess)
        {
            Emit("business", "vacation.approved", $"Ausencia {id} aprobada", id);
        }

        return result;
    }

    public async Task<Result<VacationResponse>> RejectAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _rejectHandler.Handle(id, cancellationToken);

        if (result.IsSuccess)
        {
            Emit("business", "vacation.rejected", $"Ausencia {id} denegada", id);
        }

        return result;
    }

    private void Emit(string category, string eventType, string message, object subjectId)
    {
        _events.Emit(new AppEvent(
            category,
            eventType,
            category == "security" ? "Warning" : "Info",
            message,
            ActorRole: _currentUser.Role,
            ActorId: _currentUser.SubjectId,
            CenterId: _currentUser.CenterId,
            Data: new Dictionary<string, object?> { ["subjectId"] = subjectId }));
    }
}
