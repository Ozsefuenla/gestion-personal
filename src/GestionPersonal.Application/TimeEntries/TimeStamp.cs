namespace GestionPersonal.Application.TimeEntries;

internal static class TimeStamp
{
    public static DateTimeOffset Compose(DateOnly date, TimeOnly time)
    {
        var local = date.ToDateTime(time);
        var offset = MadridTimeZone.Instance.GetUtcOffset(local);

        return new DateTimeOffset(local, offset);
    }
}
