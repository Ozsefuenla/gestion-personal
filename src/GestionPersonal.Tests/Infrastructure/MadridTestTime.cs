namespace GestionPersonal.Tests.Infrastructure;

public static class MadridTestTime
{
    private static readonly TimeZoneInfo Zone = Resolve();

    public static DateOnly Today() =>
        DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, Zone).DateTime);

    public static (int Year, int Month) CurrentMonth()
    {
        var local = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, Zone);
        return (local.Year, local.Month);
    }

    private static TimeZoneInfo Resolve()
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
