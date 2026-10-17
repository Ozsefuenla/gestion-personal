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

    public VacationsService(
        ListVacationsQueryHandler listHandler,
        ListCenterVacationsQueryHandler listCenterHandler,
        RequestVacationCommandHandler requestHandler,
        DeleteVacationCommandHandler deleteHandler,
        ApproveVacationCommandHandler approveHandler,
        RejectVacationCommandHandler rejectHandler,
        IRequestValidator requestValidator)
    {
        _listHandler = listHandler;
        _listCenterHandler = listCenterHandler;
        _requestHandler = requestHandler;
        _deleteHandler = deleteHandler;
        _approveHandler = approveHandler;
        _rejectHandler = rejectHandler;
        _requestValidator = requestValidator;
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

        return await _requestHandler.Handle(command, cancellationToken);
    }

    public Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default) =>
        _deleteHandler.Handle(id, cancellationToken);

    public Task<Result<VacationResponse>> ApproveAsync(Guid id, CancellationToken cancellationToken = default) =>
        _approveHandler.Handle(id, cancellationToken);

    public Task<Result<VacationResponse>> RejectAsync(Guid id, CancellationToken cancellationToken = default) =>
        _rejectHandler.Handle(id, cancellationToken);
}
