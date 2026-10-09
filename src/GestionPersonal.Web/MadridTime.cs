namespace GestionPersonal.Web;

public static class MadridTime
{
    private static readonly TimeZoneInfo Zone = Resolve();

    public static string Format(DateTimeOffset value)
    {
        var local = TimeZoneInfo.ConvertTime(value, Zone);
        return local.ToString("HH:mm:ss");
    }

    public static TimeSpan TimeOfDay(DateTimeOffset value)
    {
        var local = TimeZoneInfo.ConvertTime(value, Zone);
        return local.TimeOfDay;
    }

    public static DateTimeOffset Combine(DateTimeOffset original, TimeSpan time)
    {
        var local = TimeZoneInfo.ConvertTime(original, Zone);
        var combined = local.Date + time;
        var offset = Zone.GetUtcOffset(combined);

        return new DateTimeOffset(combined, offset);
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
