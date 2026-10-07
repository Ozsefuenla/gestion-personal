using GestionPersonal.Application.Abstractions;
using GestionPersonal.Application.Common;
using GestionPersonal.Application.TimeEntries;

namespace GestionPersonal.Application.Workers;

public sealed class GetWorkersStatusQueryHandler
{
    private readonly IWorkerRepository _workerRepository;
    private readonly ITimeEntryRepository _timeEntryRepository;

    public GetWorkersStatusQueryHandler(
        IWorkerRepository workerRepository,
        ITimeEntryRepository timeEntryRepository)
    {
        _workerRepository = workerRepository;
        _timeEntryRepository = timeEntryRepository;
    }

    public async Task<Result<IReadOnlyList<WorkerStatusResponse>>> Handle(
        CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        var (from, to) = TodayStatusHelper.GetTodayRange(now);

        var workers = await _workerRepository.GetAllAsync(cancellationToken);

        var result = new List<WorkerStatusResponse>();

        foreach (var worker in workers)
        {
            var openEntry = await _timeEntryRepository.GetOpenEntryByWorkerAsync(worker.Id, cancellationToken);
            var entries = await _timeEntryRepository.GetByWorkerBetweenAsync(worker.Id, from, to, cancellationToken);

            result.Add(new WorkerStatusResponse(
                worker.Id,
                worker.FullName,
                worker.Role.ToString().ToLowerInvariant(),
                TodayStatusHelper.GetCurrentStatus(openEntry, entries),
                openEntry?.StartedAt,
                openEntry?.CurrentPausedAt,
                entries.Aggregate(TimeSpan.Zero, (total, e) => total + e.GetWorkedDuration(now)),
                entries.Aggregate(TimeSpan.Zero, (total, e) => total + e.GetPausedDuration(now)),
                TodayStatusHelper.BuildTimeline(entries, now)));
        }

        return Result<IReadOnlyList<WorkerStatusResponse>>.Success(result);
    }
}
