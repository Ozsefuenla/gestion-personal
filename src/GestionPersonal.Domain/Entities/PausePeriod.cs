namespace GestionPersonal.Domain.Entities;

public sealed class PausePeriod
{
    public DateTimeOffset PausedAt { get; }

    public DateTimeOffset? ResumedAt { get; private set; }

    internal PausePeriod(DateTimeOffset pausedAt)
    {
        PausedAt = pausedAt;
    }

    internal void Close(DateTimeOffset now)
    {
        ResumedAt = now;
    }
}
