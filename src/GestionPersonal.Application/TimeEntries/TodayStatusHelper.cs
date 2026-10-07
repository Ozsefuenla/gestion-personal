using GestionPersonal.Domain.Entities;
using GestionPersonal.Domain.Enums;

namespace GestionPersonal.Application.TimeEntries;

internal static class TodayStatusHelper
{
    public static (DateTimeOffset From, DateTimeOffset To) GetTodayRange(DateTimeOffset now)
    {
        var localNow = TimeZoneInfo.ConvertTime(now, MadridTimeZone.Instance);
        var localStart = localNow.Date;
        var offset = MadridTimeZone.Instance.GetUtcOffset(localStart);

        var from = new DateTimeOffset(localStart, offset);

        return (from, from.AddDays(1));
    }

    public static string GetCurrentStatus(TimeEntry? openEntry, IReadOnlyList<TimeEntry> entries)
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

    public static IReadOnlyList<TimelineSegmentResponse> BuildTimeline(
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
}
