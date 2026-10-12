using GestionPersonal.Application.Abstractions;
using GestionPersonal.Application.Common;

namespace GestionPersonal.Application.Workers;

public sealed class ListWorkersByCenterQueryHandler
{
    private readonly ICenterRepository _centerRepository;
    private readonly IWorkerRepository _workerRepository;

    public ListWorkersByCenterQueryHandler(
        ICenterRepository centerRepository,
        IWorkerRepository workerRepository)
    {
        _centerRepository = centerRepository;
        _workerRepository = workerRepository;
    }

    public async Task<Result<IReadOnlyList<WorkerResponse>>> Handle(
        ListWorkersByCenterQuery query,
        CancellationToken cancellationToken = default)
    {
        var center = await _centerRepository.GetByIdAsync(query.CenterId, cancellationToken);

        if (center is null)
        {
            return Error.NotFound("El centro no existe.");
        }

        var workers = await _workerRepository.GetByCenterAsync(query.CenterId, cancellationToken);

        IReadOnlyList<WorkerResponse> response = workers
            .Select(WorkerResponse.From)
            .ToList();

        return Result<IReadOnlyList<WorkerResponse>>.Success(response);
    }
}
