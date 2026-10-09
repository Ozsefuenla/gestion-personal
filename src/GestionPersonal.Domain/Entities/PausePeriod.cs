namespace GestionPersonal.Domain.Entities;

public sealed class PausePeriod
{
    public DateTimeOffset PausedAt { get; private set; }

    public DateTimeOffset? ResumedAt { get; private set; }

    internal PausePeriod(DateTimeOffset pausedAt)
    {
        PausedAt = pausedAt;
    }

    internal void Close(DateTimeOffset now)
    {
        ResumedAt = now;
    }

    internal void Reopen()
    {
        ResumedAt = null;
    }

    internal void EditPausedAt(DateTimeOffset pausedAt)
    {
        PausedAt = pausedAt;
    }

    internal void EditResumedAt(DateTimeOffset resumedAt)
    {
        ResumedAt = resumedAt;
    }
}
