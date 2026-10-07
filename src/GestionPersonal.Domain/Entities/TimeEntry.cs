using GestionPersonal.Domain.Enums;

namespace GestionPersonal.Domain.Entities;

public sealed class TimeEntry
{
    private readonly List<PausePeriod> _pauses = [];

    public Guid Id { get; private set; }
    public Guid WorkerId { get; private set; }
    public TimeEntryStatus Status { get; private set; }
    public DateTimeOffset StartedAt { get; private set; }
    public DateTimeOffset? EndedAt { get; private set; }
    public IReadOnlyList<PausePeriod> Pauses => _pauses;

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

        _pauses.Add(new PausePeriod(now));
        Status = TimeEntryStatus.Paused;
    }

    public void Resume(DateTimeOffset now)
    {
        if (Status != TimeEntryStatus.Paused)
        {
            throw new InvalidOperationException("Solo se puede reanudar un fichaje en pausa.");
        }

        OpenPause.Close(now);
        Status = TimeEntryStatus.InProgress;
    }

    public void End(DateTimeOffset now)
    {
        if (Status != TimeEntryStatus.InProgress && Status != TimeEntryStatus.Paused)
        {
            throw new InvalidOperationException("No se puede finalizar un fichaje que no está en curso.");
        }

        if (Status == TimeEntryStatus.Paused)
        {
            OpenPause.Close(now);
        }

        Status = TimeEntryStatus.Finished;
        EndedAt = now;
    }

    public DateTimeOffset? CurrentPausedAt =>
        Status == TimeEntryStatus.Paused ? OpenPause.PausedAt : null;

    public TimeSpan GetPausedDuration(DateTimeOffset? now = null)
    {
        var endReference = EndedAt ?? now ?? DateTimeOffset.UtcNow;

        return _pauses.Aggregate(TimeSpan.Zero, (total, pause) =>
        {
            var pauseEnd = pause.ResumedAt ?? endReference;
            return pauseEnd > pause.PausedAt
                ? total + (pauseEnd - pause.PausedAt)
                : total;
        });
    }

    public TimeSpan GetWorkedDuration(DateTimeOffset? now = null)
    {
        var endReference = EndedAt ?? now ?? DateTimeOffset.UtcNow;
        var raw = endReference - StartedAt;

        return raw > TimeSpan.Zero ? raw - GetPausedDuration(now) : TimeSpan.Zero;
    }

    private PausePeriod OpenPause => _pauses.First(p => p.ResumedAt is null);
}
