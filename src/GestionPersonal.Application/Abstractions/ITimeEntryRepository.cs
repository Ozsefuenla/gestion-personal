using GestionPersonal.Domain.Entities;

namespace GestionPersonal.Application.Abstractions;

public interface ITimeEntryRepository
{
    Task<TimeEntry?> GetOpenEntryByWorkerAsync(int workerId, CancellationToken cancellationToken = default);

    Task<TimeEntry?> GetOpenEntryByWorkerBetweenAsync(
        int workerId,
        DateTimeOffset from,
        DateTimeOffset to,
        CancellationToken cancellationToken = default);

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

    Task AddAsync(TimeEntry timeEntry, CancellationToken cancellationToken = default);

    Task UpdateAsync(TimeEntry timeEntry, CancellationToken cancellationToken = default);

    Task RemoveAsync(TimeEntry timeEntry, CancellationToken cancellationToken = default);
}
