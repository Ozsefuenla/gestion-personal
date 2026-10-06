using GestionPersonal.Application.Abstractions;
using GestionPersonal.Application.Common;

namespace GestionPersonal.Application.Workers;

public sealed class ListWorkersQueryHandler
{
    private readonly IWorkerRepository _workerRepository;

    public ListWorkersQueryHandler(IWorkerRepository workerRepository)
    {
        _workerRepository = workerRepository;
    }

    public async Task<Result<IReadOnlyList<WorkerResponse>>> Handle(
        ListWorkersQuery query,
        CancellationToken cancellationToken = default)
    {
        var workers = await _workerRepository.GetAllAsync(cancellationToken);

        IReadOnlyList<WorkerResponse> response = workers
            .Select(WorkerResponse.From)
            .ToList();

        return Result<IReadOnlyList<WorkerResponse>>.Success(response);
    }
}
