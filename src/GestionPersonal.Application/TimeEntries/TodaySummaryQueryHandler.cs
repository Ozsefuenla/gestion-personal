using GestionPersonal.Application.Abstractions;
using GestionPersonal.Application.Common;
using GestionPersonal.Domain.Entities;
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
        var (from, to) = GetTodayRange(now);

        var entries = await _timeEntryRepository.GetByWorkerBetweenAsync(query.WorkerId, from, to, cancellationToken);
        var openEntry = await _timeEntryRepository.GetOpenEntryByWorkerAsync(query.WorkerId, cancellationToken);

        var response = new TodaySummaryResponse(
            CurrentStatus: GetCurrentStatus(openEntry, entries),
            StartedAt: openEntry?.StartedAt,
            CurrentPausedAt: openEntry?.CurrentPausedAt,
            WorkedTime: entries.Aggregate(TimeSpan.Zero, (total, e) => total + e.GetWorkedDuration(now)),
            PausedTime: entries.Aggregate(TimeSpan.Zero, (total, e) => total + e.GetPausedDuration(now)),
            PauseCount: entries.Sum(e => e.Pauses.Count),
            CompletedEntries: entries.Count(e => e.Status == TimeEntryStatus.Finished),
            Timeline: BuildTimeline(entries, now));

        return response;
    }

    private static (DateTimeOffset From, DateTimeOffset To) GetTodayRange(DateTimeOffset now)
    {
        var localNow = TimeZoneInfo.ConvertTime(now, MadridTimeZone.Instance);
        var localStart = localNow.Date;
        var offset = MadridTimeZone.Instance.GetUtcOffset(localStart);

        var from = new DateTimeOffset(localStart, offset);

        return (from, from.AddDays(1));
    }

    private static IReadOnlyList<TimelineSegmentResponse> BuildTimeline(
        IReadOnlyList<TimeEntry> entries,
        DateTimeOffset now)
    {
        var segments = new List<TimelineSegmentResponse>();

        foreach (var entry in entries.OrderBy(e => e.StartedAt))
        {
            var endReference = entry.EndedAt ?? now;
            var cursor = entry.StartedAt;

            foreach (var pause in entry.Pauses)
            {
                if (pause.PausedAt > cursor)
                {
                    segments.Add(new TimelineSegmentResponse("working", cursor, pause.PausedAt));
                }

                segments.Add(new TimelineSegmentResponse("paused", pause.PausedAt, pause.ResumedAt ?? endReference));
                cursor = pause.ResumedAt ?? endReference;
            }

            if (endReference > cursor)
            {
                segments.Add(new TimelineSegmentResponse("working", cursor, endReference));
            }
        }

        return segments;
    }

    private static string GetCurrentStatus(TimeEntry? openEntry, IReadOnlyList<TimeEntry> entries)
    {
        if (openEntry is not null)
        {
            return openEntry.Status switch
            {
                TimeEntryStatus.Paused => "paused",
                _ => "in-progress"
            };
        }

        return entries.Count > 0 ? "finished" : "idle";
    }
}
