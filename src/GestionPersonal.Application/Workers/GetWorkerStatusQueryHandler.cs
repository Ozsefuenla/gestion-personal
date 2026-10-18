using GestionPersonal.Application.Abstractions;
using GestionPersonal.Application.Common;
using GestionPersonal.Application.TimeEntries;

namespace GestionPersonal.Application.Workers;

public sealed class GetWorkerStatusQueryHandler
{
    private readonly IWorkerRepository _workerRepository;
    private readonly ITimeEntryRepository _timeEntryRepository;
    private readonly ICurrentUser _currentUser;

    public GetWorkerStatusQueryHandler(
        IWorkerRepository workerRepository,
        ITimeEntryRepository timeEntryRepository,
        ICurrentUser currentUser)
    {
        _workerRepository = workerRepository;
        _timeEntryRepository = timeEntryRepository;
        _currentUser = currentUser;
    }

    public async Task<Result<WorkerStatusResponse>> Handle(
        int workerId,
        CancellationToken cancellationToken = default)
    {
        var scoped = await WorkerScope.GetSelfScopedWorkerAsync(
            _workerRepository, _currentUser, workerId, cancellationToken);

        if (scoped.IsFailure)
        {
            return scoped.Error;
        }

        var now = DateTimeOffset.UtcNow;
        var (from, to) = TodayStatusHelper.GetTodayRange(now);

        return await WorkerStatusBuilder.BuildAsync(scoped.Value, now, from, to, _timeEntryRepository, cancellationToken);
    }
}
