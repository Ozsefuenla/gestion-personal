using GestionPersonal.Domain.Entities;

namespace GestionPersonal.Application.Abstractions;

public interface ITimeEntryRepository
{
    Task<TimeEntry?> GetOpenEntryByWorkerAsync(int workerId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TimeEntry>> GetByWorkerBetweenAsync(
        int workerId,
        DateTimeOffset from,
        DateTimeOffset to,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TimeEntry>> GetByWorkerOverlappingAsync(
        int workerId,
        DateTimeOffset from,
        DateTimeOffset to,
        CancellationToken cancellationToken = default);

    void Add(TimeEntry timeEntry);

    void Remove(TimeEntry timeEntry);
}
