using GestionPersonal.Application.Abstractions;
using GestionPersonal.Application.Common;

namespace GestionPersonal.Application.Workers;

public sealed class ListWorkersQueryHandler
{
    private readonly IWorkerRepository _workerRepository;
    private readonly ICurrentCenter _currentCenter;

    public ListWorkersQueryHandler(IWorkerRepository workerRepository, ICurrentCenter currentCenter)
    {
        _workerRepository = workerRepository;
        _currentCenter = currentCenter;
    }

    public async Task<Result<IReadOnlyList<WorkerResponse>>> Handle(
        ListWorkersQuery query,
        CancellationToken cancellationToken = default)
    {
        var centerId = _currentCenter.CenterId;

        if (centerId is null)
        {
            return Error.Forbidden("No autenticado.");
        }

        var workers = await _workerRepository.GetByCenterAsync(centerId.Value, cancellationToken);

        IReadOnlyList<WorkerResponse> response = workers
            .Where(w => w.Active)
            .Select(WorkerResponse.From)
            .ToList();

        return Result<IReadOnlyList<WorkerResponse>>.Success(response);
    }
}
