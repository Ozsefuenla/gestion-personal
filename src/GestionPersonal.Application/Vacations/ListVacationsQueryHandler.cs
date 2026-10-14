using GestionPersonal.Application.Abstractions;
using GestionPersonal.Application.Common;

namespace GestionPersonal.Application.Vacations;

public sealed class ListVacationsQueryHandler
{
    private readonly IWorkerRepository _workerRepository;
    private readonly IVacationRepository _vacationRepository;
    private readonly ICurrentCenter _currentCenter;

    public ListVacationsQueryHandler(
        IWorkerRepository workerRepository,
        IVacationRepository vacationRepository,
        ICurrentCenter currentCenter)
    {
        _workerRepository = workerRepository;
        _vacationRepository = vacationRepository;
        _currentCenter = currentCenter;
    }

    public async Task<Result<IReadOnlyList<VacationResponse>>> Handle(
        ListVacationsQuery query,
        CancellationToken cancellationToken = default)
    {
        var workerResult = await WorkerScope.GetScopedWorkerAsync(
            _workerRepository, _currentCenter, query.WorkerId, cancellationToken);

        if (workerResult.IsFailure)
        {
            return workerResult.Error;
        }

        var vacations = await _vacationRepository.GetByWorkerAsync(query.WorkerId, cancellationToken);

        IReadOnlyList<VacationResponse> response = vacations
            .Select(VacationResponse.From)
            .ToList();

        return Result<IReadOnlyList<VacationResponse>>.Success(response);
    }
}
