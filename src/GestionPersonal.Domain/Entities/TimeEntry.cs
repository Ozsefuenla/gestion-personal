using GestionPersonal.Domain.Enums;

namespace GestionPersonal.Domain.Entities;

public sealed class TimeEntry
{
    public Guid Id { get; private set; }
    public Guid WorkerId { get; private set; }
    public TimeEntryStatus Status { get; private set; }
    public DateTimeOffset StartedAt { get; private set; }
    public DateTimeOffset? PausedAt { get; private set; }
    public DateTimeOffset? EndedAt { get; private set; }

    private TimeEntry()
    {
    }

    public static TimeEntry Start(Guid workerId, DateTimeOffset now)
    {
        return new TimeEntry
        {
            Id = Guid.NewGuid(),
            WorkerId = workerId,
            Status = TimeEntryStatus.InProgress,
            StartedAt = now
        };
    }

    public void Pause(DateTimeOffset now)
    {
        if (Status != TimeEntryStatus.InProgress)
        {
            throw new InvalidOperationException("Solo se puede pausar un fichaje en curso.");
        }

        Status = TimeEntryStatus.Paused;
        PausedAt = now;
    }

    public void Resume(DateTimeOffset now)
    {
        if (Status != TimeEntryStatus.Paused)
        {
            throw new InvalidOperationException("Solo se puede reanudar un fichaje en pausa.");
        }

        Status = TimeEntryStatus.InProgress;
        PausedAt = null;
    }

    public void End(DateTimeOffset now)
    {
        if (Status == TimeEntryStatus.Finished)
        {
            throw new InvalidOperationException("El fichaje ya está finalizado.");
        }

        Status = TimeEntryStatus.Finished;
        EndedAt = now;
    }
}
