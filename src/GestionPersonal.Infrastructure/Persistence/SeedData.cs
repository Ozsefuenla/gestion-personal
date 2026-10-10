using GestionPersonal.Domain.Entities;

namespace GestionPersonal.Infrastructure.Persistence;

internal static class SeedData
{
    private static readonly TimeZoneInfo Madrid = ResolveMadrid();

    public static IReadOnlyList<TimeEntry> BuildTimeEntries()
    {
        var random = new Random(2026);
        var deltas = new[] { -60, -45, -30, -15, 0, 0, 15, 30, 45, 60 };
        var workerIds = new[] { 1, 2, 3 };
        var entries = new List<TimeEntry>();
        var today = DateTime.Today;
        var deltaIndex = 0;

        foreach (var month in new[] { 9, 10 })
        {
            var days = DateTime.DaysInMonth(2026, month);

            for (var day = 1; day <= days; day++)
            {
                var date = new DateTime(2026, month, day);

                if (date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
                {
                    continue;
                }

                if (date > today)
                {
                    continue;
                }

                foreach (var workerId in workerIds)
                {
                    var delta = deltas[deltaIndex++ % deltas.Length];
                    var worked = TimeSpan.FromHours(8) + TimeSpan.FromMinutes(delta);
                    var pauseMinutes = random.Next(4) == 0 ? random.Next(15, 46) : 0;

                    entries.Add(BuildEntry(workerId, date, worked, TimeSpan.FromMinutes(pauseMinutes)));
                }
            }
        }

        return entries;
    }

    private static TimeEntry BuildEntry(int workerId, DateTime date, TimeSpan worked, TimeSpan pause)
    {
        var startLocal = date.AddHours(9);
        var entry = TimeEntry.Start(workerId, ToOffset(startLocal));

        if (pause > TimeSpan.Zero)
        {
            var pauseStartLocal = date.AddHours(13);
            entry.Pause(ToOffset(pauseStartLocal));
            entry.Resume(ToOffset(pauseStartLocal.Add(pause)));
        }

        entry.End(ToOffset(startLocal.Add(worked).Add(pause)));

        return entry;
    }

    private static DateTimeOffset ToOffset(DateTime local)
    {
        var offset = Madrid.GetUtcOffset(local);
        return new DateTimeOffset(local, offset);
    }

    private static TimeZoneInfo ResolveMadrid()
    {
        foreach (var id in new[] { "Europe/Madrid", "Romance Standard Time" })
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById(id);
            }
            catch (TimeZoneNotFoundException)
            {
            }
            catch (InvalidTimeZoneException)
            {
            }
        }

        throw new TimeZoneNotFoundException("No se encontró la zona horaria de Madrid.");
    }
}
