using GestionPersonal.Application.Abstractions;
using GestionPersonal.Application.Common;
using GestionPersonal.Application.TimeEntries;

namespace GestionPersonal.Application.Workers;

public sealed class GetWorkersStatusQueryHandler
{
    private readonly IWorkerRepository _workerRepository;
    private readonly ITimeEntryRepository _timeEntryRepository;
    private readonly ICurrentUser _currentUser;

    public GetWorkersStatusQueryHandler(
        IWorkerRepository workerRepository,
        ITimeEntryRepository timeEntryRepository,
        ICurrentUser currentUser)
    {
        _workerRepository = workerRepository;
        _timeEntryRepository = timeEntryRepository;
        _currentUser = currentUser;
    }

    public async Task<Result<IReadOnlyList<WorkerStatusResponse>>> Handle(
        CancellationToken cancellationToken = default)
    {
        if (AccessControl.RequireCenterAdmin(_currentUser) is { } error)
        {
            return error;
        }

        var now = DateTimeOffset.UtcNow;
        var (from, to) = TodayStatusHelper.GetTodayRange(now);

        var workers = await _workerRepository.GetByCenterAsync(_currentUser.CenterId!.Value, cancellationToken);

        var result = new List<WorkerStatusResponse>();

        foreach (var worker in workers.Where(w => w.Active))
        {
            result.Add(await WorkerStatusBuilder.BuildAsync(worker, now, from, to, _timeEntryRepository, cancellationToken));
        }

        return Result<IReadOnlyList<WorkerStatusResponse>>.Success(result);
    }
}
