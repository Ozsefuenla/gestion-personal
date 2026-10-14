using GestionPersonal.Application.Abstractions;
using GestionPersonal.Application.Common;

namespace GestionPersonal.Application.Vacations;

public sealed class ListCenterVacationsQueryHandler
{
    private readonly IWorkerRepository _workerRepository;
    private readonly IVacationRepository _vacationRepository;
    private readonly ICurrentCenter _currentCenter;

    public ListCenterVacationsQueryHandler(
        IWorkerRepository workerRepository,
        IVacationRepository vacationRepository,
        ICurrentCenter currentCenter)
    {
        _workerRepository = workerRepository;
        _vacationRepository = vacationRepository;
        _currentCenter = currentCenter;
    }

    public async Task<Result<IReadOnlyList<VacationResponse>>> Handle(
        ListCenterVacationsQuery query,
        CancellationToken cancellationToken = default)
    {
        var centerId = _currentCenter.CenterId;

        if (centerId is null)
        {
            return Error.Forbidden("No autenticado.");
        }

        var workers = await _workerRepository.GetByCenterAsync(centerId.Value, cancellationToken);
        var workerIds = workers.Select(w => w.Id).ToList();

        var vacations = await _vacationRepository.GetByWorkerIdsAsync(workerIds, cancellationToken);

        IReadOnlyList<VacationResponse> response = vacations
            .Select(VacationResponse.From)
            .ToList();

        return Result<IReadOnlyList<VacationResponse>>.Success(response);
    }
}
