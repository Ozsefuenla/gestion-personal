using GestionPersonal.Domain.Entities;

namespace GestionPersonal.Application.Abstractions;

public interface ITimeEntryRepository
{
    Task<TimeEntry?> GetOpenEntryByWorkerAsync(Guid workerId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TimeEntry>> GetByWorkerBetweenAsync(
        Guid workerId,
        DateTimeOffset from,
        DateTimeOffset to,
        CancellationToken cancellationToken = default);

    void Add(TimeEntry timeEntry);

    void Remove(TimeEntry timeEntry);
}
