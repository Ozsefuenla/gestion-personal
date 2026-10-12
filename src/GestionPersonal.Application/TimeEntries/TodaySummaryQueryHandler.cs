using GestionPersonal.Application.Abstractions;
using GestionPersonal.Application.Common;
using GestionPersonal.Domain.Enums;

namespace GestionPersonal.Application.TimeEntries;

public sealed class TodaySummaryQueryHandler
{
    private readonly IWorkerRepository _workerRepository;
    private readonly ICurrentCenter _currentCenter;
    private readonly ITimeEntryRepository _timeEntryRepository;

    public TodaySummaryQueryHandler(
        IWorkerRepository workerRepository,
        ICurrentCenter currentCenter,
        ITimeEntryRepository timeEntryRepository)
    {
        _workerRepository = workerRepository;
        _currentCenter = currentCenter;
        _timeEntryRepository = timeEntryRepository;
    }

    public async Task<Result<TodaySummaryResponse>> Handle(
        TodaySummaryQuery query,
        CancellationToken cancellationToken = default)
    {
        var workerResult = await WorkerScope.GetScopedWorkerAsync(_workerRepository, _currentCenter, query.WorkerId, cancellationToken);

        if (workerResult.IsFailure)
        {
            return workerResult.Error;
        }

        var now = DateTimeOffset.UtcNow;
        var (from, to) = TodayStatusHelper.GetTodayRange(now);

        var entries = await _timeEntryRepository.GetByWorkerBetweenAsync(query.WorkerId, from, to, cancellationToken);
        var openEntry = await _timeEntryRepository.GetOpenEntryByWorkerAsync(query.WorkerId, cancellationToken);

        var response = new TodaySummaryResponse(
            CurrentStatus: TodayStatusHelper.GetCurrentStatus(openEntry, entries),
            StartedAt: openEntry?.StartedAt,
            CurrentPausedAt: openEntry?.CurrentPausedAt,
            WorkedTime: entries.Aggregate(TimeSpan.Zero, (total, e) => total + e.GetWorkedDuration(now)),
            PausedTime: entries.Aggregate(TimeSpan.Zero, (total, e) => total + e.GetPausedDuration(now)),
            PauseCount: entries.Sum(e => e.Pauses.Count),
            CompletedEntries: entries.Count(e => e.Status == TimeEntryStatus.Finished),
            Timeline: TodayStatusHelper.BuildTimeline(entries, now));

        return response;
    }
}
