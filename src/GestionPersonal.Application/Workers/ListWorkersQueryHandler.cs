using GestionPersonal.Application.Abstractions;
using GestionPersonal.Application.Common;

namespace GestionPersonal.Application.Workers;

public sealed class ListWorkersQueryHandler
{
    private readonly IWorkerRepository _workerRepository;
    private readonly ICurrentUser _currentUser;

    public ListWorkersQueryHandler(IWorkerRepository workerRepository, ICurrentUser currentUser)
    {
        _workerRepository = workerRepository;
        _currentUser = currentUser;
    }

    public async Task<Result<IReadOnlyList<WorkerResponse>>> Handle(
        ListWorkersQuery query,
        CancellationToken cancellationToken = default)
    {
        if (AccessControl.RequireCenterMember(_currentUser) is { } error)
        {
            return error;
        }

        var workers = await _workerRepository.GetByCenterAsync(_currentUser.CenterId!.Value, cancellationToken);

        IReadOnlyList<WorkerResponse> response = workers
            .Where(w => w.Active)
            .Select(WorkerResponse.From)
            .ToList();

        return Result<IReadOnlyList<WorkerResponse>>.Success(response);
    }
}
