using GestionPersonal.Domain.Entities;

namespace GestionPersonal.Application.TimeEntries;

internal static class MonthlySummaryHelper
{
    public static (DateTimeOffset From, DateTimeOffset To) GetMonthRange(int year, int month)
    {
        var localStart = new DateTime(year, month, 1);
        return (ToMadridOffset(localStart), ToMadridOffset(localStart.AddMonths(1)));
    }

    public static (DateTimeOffset From, DateTimeOffset To) GetDayRange(int year, int month, int day)
    {
        var localDay = new DateTime(year, month, day);
        return (ToMadridOffset(localDay), ToMadridOffset(localDay.AddDays(1)));
    }

    public static int DaysInMonth(int year, int month) => DateTime.DaysInMonth(year, month);

    public static IReadOnlyList<(DateTimeOffset From, DateTimeOffset To)> GetWorkingSegments(
        TimeEntry entry,
        DateTimeOffset now)
    {
        var segments = new List<(DateTimeOffset From, DateTimeOffset To)>();
        var endReference = entry.EndedAt ?? now;
        var cursor = entry.StartedAt;

        foreach (var pause in entry.Pauses)
        {
            if (pause.PausedAt > cursor)
            {
                segments.Add((cursor, pause.PausedAt));
            }

            cursor = pause.ResumedAt ?? endReference;
        }

        if (endReference > cursor)
        {
            segments.Add((cursor, endReference));
        }

        return segments;
    }

    public static IReadOnlyList<(DateTimeOffset From, DateTimeOffset To)> GetPausedSegments(
        TimeEntry entry,
        DateTimeOffset now)
    {
        var endReference = entry.EndedAt ?? now;
        var segments = new List<(DateTimeOffset From, DateTimeOffset To)>();

        foreach (var pause in entry.Pauses)
        {
            var pauseEnd = pause.ResumedAt ?? endReference;

            if (pauseEnd > pause.PausedAt)
            {
                segments.Add((pause.PausedAt, pauseEnd));
            }
        }

        return segments;
    }

    public static IReadOnlyList<(string Type, DateTimeOffset From, DateTimeOffset To)> GetDayTimeline(
        TimeEntry entry,
        DateTimeOffset dayFrom,
        DateTimeOffset dayTo,
        DateTimeOffset now)
    {
        var segments = new List<(string Type, DateTimeOffset From, DateTimeOffset To)>();

        foreach (var segment in GetWorkingSegments(entry, now))
        {
            AddClipped(segments, "working", segment.From, segment.To, dayFrom, dayTo);
        }

        foreach (var segment in GetPausedSegments(entry, now))
        {
            AddClipped(segments, "paused", segment.From, segment.To, dayFrom, dayTo);
        }

        return segments.OrderBy(s => s.From).ToList();
    }

    private static void AddClipped(
        List<(string Type, DateTimeOffset From, DateTimeOffset To)> target,
        string type,
        DateTimeOffset from,
        DateTimeOffset to,
        DateTimeOffset dayFrom,
        DateTimeOffset dayTo)
    {
        var start = from > dayFrom ? from : dayFrom;
        var end = to < dayTo ? to : dayTo;

        if (end > start)
        {
            target.Add((type, start, end));
        }
    }

    private static DateTimeOffset ToMadridOffset(DateTime local)
    {
        var offset = MadridTimeZone.Instance.GetUtcOffset(local);
        return new DateTimeOffset(local, offset);
    }
}
