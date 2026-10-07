namespace GestionPersonal.Application.TimeEntries;

internal static class MadridTimeZone
{
    public static TimeZoneInfo Instance { get; } = Resolve();

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

        throw new TimeZoneNotFoundException(
            "No se encontró la zona horaria de Madrid (Europe/Madrid / Romance Standard Time).");
    }
}
