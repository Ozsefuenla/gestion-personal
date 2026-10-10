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

    private static DateTimeOffset ToMadridOffset(DateTime local)
    {
        var offset = MadridTimeZone.Instance.GetUtcOffset(local);
        return new DateTimeOffset(local, offset);
    }
}
