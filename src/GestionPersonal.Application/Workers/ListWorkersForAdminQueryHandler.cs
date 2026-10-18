using GestionPersonal.Application.Abstractions;
using GestionPersonal.Application.Common;

namespace GestionPersonal.Application.Workers;

public sealed class ListWorkersForAdminQueryHandler
{
    private readonly IWorkerRepository _workerRepository;
    private readonly ICurrentUser _currentUser;

    public ListWorkersForAdminQueryHandler(
        IWorkerRepository workerRepository,
        ICurrentUser currentUser)
    {
        _workerRepository = workerRepository;
        _currentUser = currentUser;
    }

    public async Task<Result<IReadOnlyList<WorkerResponse>>> Handle(
        ListWorkersForAdminQuery query,
        CancellationToken cancellationToken = default)
    {
        if (AccessControl.RequireCenterAdmin(_currentUser) is { } error)
        {
            return error;
        }

        var workers = await _workerRepository.GetByCenterAsync(_currentUser.CenterId!.Value, cancellationToken);

        IReadOnlyList<WorkerResponse> response = workers
            .Select(WorkerResponse.From)
            .ToList();

        return Result<IReadOnlyList<WorkerResponse>>.Success(response);
    }
}
