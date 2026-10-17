using GestionPersonal.Application.Common;
using GestionPersonal.Application.Vacations;

namespace GestionPersonal.Application.Services;

public interface IVacationsService
{
    Task<Result<IReadOnlyList<VacationResponse>>> ListAsync(
        ListVacationsQuery query,
        CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyList<VacationResponse>>> ListCenterAsync(CancellationToken cancellationToken = default);

    Task<Result<VacationResponse>> RequestAsync(
        RequestVacationCommand command,
        CancellationToken cancellationToken = default);

    Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Result<VacationResponse>> ApproveAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Result<VacationResponse>> RejectAsync(Guid id, CancellationToken cancellationToken = default);
}
