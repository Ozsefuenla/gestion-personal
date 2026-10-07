using GestionPersonal.Application.Abstractions;
using GestionPersonal.Application.Common;
using GestionPersonal.Domain.Enums;

namespace GestionPersonal.Application.TimeEntries;

public sealed class TodaySummaryQueryHandler
{
    private readonly ITimeEntryRepository _timeEntryRepository;

    public TodaySummaryQueryHandler(ITimeEntryRepository timeEntryRepository)
    {
        _timeEntryRepository = timeEntryRepository;
    }

    public async Task<Result<TodaySummaryResponse>> Handle(
        TodaySummaryQuery query,
        CancellationToken cancellationToken = default)
    {
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
