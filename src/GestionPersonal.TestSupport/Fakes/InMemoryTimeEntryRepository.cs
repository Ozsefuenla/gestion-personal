using GestionPersonal.Application.Abstractions;
using GestionPersonal.Domain.Entities;
using GestionPersonal.Domain.Enums;
using GestionPersonal.Infrastructure.Persistence;

namespace GestionPersonal.TestSupport.Fakes;

public sealed class InMemoryTimeEntryRepository : ITimeEntryRepository
{
    private readonly List<TimeEntry> _entries = [.. SeedData.BuildTimeEntries()];
    private readonly Lock _gate = new();

    public Task<TimeEntry?> GetOpenEntryByWorkerAsync(int workerId, CancellationToken cancellationToken = default)
    {
        lock (_gate)
        {
            return Task.FromResult(_entries.FirstOrDefault(e =>
                e.WorkerId == workerId &&
                (e.Status == TimeEntryStatus.InProgress || e.Status == TimeEntryStatus.Paused)));
        }
    }

    public Task<IReadOnlyList<TimeEntry>> GetByWorkerBetweenAsync(
        int workerId,
        DateTimeOffset from,
        DateTimeOffset to,
        CancellationToken cancellationToken = default)
    {
        lock (_gate)
        {
            IReadOnlyList<TimeEntry> result = _entries
                .Where(e => e.WorkerId == workerId && e.StartedAt >= from && e.StartedAt < to)
                .OrderBy(e => e.StartedAt)
                .ToList();

            return Task.FromResult(result);
        }
    }

    public Task<IReadOnlyList<TimeEntry>> GetByWorkerOverlappingAsync(
        int workerId,
        DateTimeOffset from,
        DateTimeOffset to,
        CancellationToken cancellationToken = default)
    {
        lock (_gate)
        {
            IReadOnlyList<TimeEntry> result = _entries
                .Where(e => e.WorkerId == workerId &&
                            e.StartedAt < to &&
                            (e.EndedAt is null || e.EndedAt >= from))
                .OrderBy(e => e.StartedAt)
                .ToList();

            return Task.FromResult(result);
        }
    }

    public Task AddAsync(TimeEntry timeEntry, CancellationToken cancellationToken = default)
    {
        lock (_gate)
        {
            _entries.Add(timeEntry);
            return Task.CompletedTask;
        }
    }

    public Task UpdateAsync(TimeEntry timeEntry, CancellationToken cancellationToken = default)
    {
        lock (_gate)
        {
            var index = _entries.FindIndex(e => e.Id == timeEntry.Id);

            if (index >= 0)
            {
                _entries[index] = timeEntry;
            }

            return Task.CompletedTask;
        }
    }

    public Task RemoveAsync(TimeEntry timeEntry, CancellationToken cancellationToken = default)
    {
        lock (_gate)
        {
            _entries.RemoveAll(e => e.Id == timeEntry.Id);
            return Task.CompletedTask;
        }
    }
}
