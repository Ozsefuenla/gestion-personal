using GestionPersonal.Application.Abstractions;
using GestionPersonal.Domain.Entities;
using GestionPersonal.Domain.Enums;
using GestionPersonal.Infrastructure.Persistence;

namespace GestionPersonal.Infrastructure.Persistence.Repositories;

public sealed class InMemoryTimeEntryRepository : ITimeEntryRepository
{
    private readonly List<TimeEntry> _entries = [.. SeedData.BuildTimeEntries()];

    public Task<TimeEntry?> GetOpenEntryByWorkerAsync(int workerId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_entries.FirstOrDefault(e =>
            e.WorkerId == workerId &&
            (e.Status == TimeEntryStatus.InProgress || e.Status == TimeEntryStatus.Paused)));
    }

    public Task<IReadOnlyList<TimeEntry>> GetByWorkerBetweenAsync(
        int workerId,
        DateTimeOffset from,
        DateTimeOffset to,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyList<TimeEntry> result = _entries
            .Where(e => e.WorkerId == workerId && e.StartedAt >= from && e.StartedAt < to)
            .OrderBy(e => e.StartedAt)
            .ToList();

        return Task.FromResult(result);
    }

    public Task<IReadOnlyList<TimeEntry>> GetByWorkerOverlappingAsync(
        int workerId,
        DateTimeOffset from,
        DateTimeOffset to,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyList<TimeEntry> result = _entries
            .Where(e => e.WorkerId == workerId &&
                        e.StartedAt < to &&
                        (e.EndedAt is null || e.EndedAt >= from))
            .OrderBy(e => e.StartedAt)
            .ToList();

        return Task.FromResult(result);
    }

    public void Add(TimeEntry timeEntry)
    {
        _entries.Add(timeEntry);
    }

    public void Remove(TimeEntry timeEntry)
    {
        _entries.Remove(timeEntry);
    }
}
