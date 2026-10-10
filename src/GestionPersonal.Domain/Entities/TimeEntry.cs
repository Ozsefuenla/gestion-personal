using GestionPersonal.Domain.Enums;

namespace GestionPersonal.Domain.Entities;

public sealed class TimeEntry
{
    private readonly List<PausePeriod> _pauses = [];

    public Guid Id { get; private set; }
    public int WorkerId { get; private set; }
    public TimeEntryStatus Status { get; private set; }
    public DateTimeOffset StartedAt { get; private set; }
    public DateTimeOffset? EndedAt { get; private set; }
    public IReadOnlyList<PausePeriod> Pauses => _pauses;

    private TimeEntry()
    {
    }

    public static TimeEntry Start(int workerId, DateTimeOffset now)
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

    public void EditLastActiveStart(DateTimeOffset newStart, DateTimeOffset now)
    {
        if (Status == TimeEntryStatus.Paused)
        {
            var previousBoundary = _pauses.Count >= 2
                ? _pauses[^2].ResumedAt!.Value
                : StartedAt;

            ValidateNewStart(newStart, previousBoundary, now);
            OpenPause.EditPausedAt(newStart);
            return;
        }

        if (Status == TimeEntryStatus.InProgress)
        {
            if (_pauses.Count > 0)
            {
                var lastPause = _pauses[^1];

                ValidateNewStart(newStart, lastPause.PausedAt, now);
                lastPause.EditResumedAt(newStart);
                return;
            }

            ValidateNewStart(newStart, DateTimeOffset.MinValue, now);
            StartedAt = newStart;
            return;
        }

        throw new InvalidOperationException("No hay un fichaje en curso que editar.");
    }

    public void DeleteLastActive()
    {
        if (Status == TimeEntryStatus.Paused)
        {
            _pauses.Remove(OpenPause);
            Status = TimeEntryStatus.InProgress;
            return;
        }

        if (Status == TimeEntryStatus.InProgress && _pauses.Count > 0)
        {
            _pauses[^1].Reopen();
            Status = TimeEntryStatus.Paused;
            return;
        }

        throw new InvalidOperationException("No hay un fichaje en curso que eliminar.");
    }

    private static void ValidateNewStart(DateTimeOffset newStart, DateTimeOffset previousBoundary, DateTimeOffset now)
    {
        if (newStart <= previousBoundary)
        {
            throw new ArgumentException("La nueva hora de inicio solapa con el tramo anterior.");
        }

        if (newStart >= now)
        {
            throw new ArgumentException("La nueva hora de inicio no puede ser posterior a la hora actual.");
        }
    }

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
