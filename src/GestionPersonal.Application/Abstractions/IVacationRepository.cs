using GestionPersonal.Domain.Entities;

namespace GestionPersonal.Application.Abstractions;

public interface IVacationRepository
{
    Task<IReadOnlyList<Vacation>> GetByWorkerAsync(int workerId, CancellationToken cancellationToken = default);

    Task<Vacation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Vacation>> GetApprovedByWorkerBetweenAsync(
        int workerId,
        DateOnly from,
        DateOnly to,
        CancellationToken cancellationToken = default);

    Task<bool> HasOverlappingActiveAsync(
        int workerId,
        DateOnly startDate,
        DateOnly endDate,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Vacation>> GetByWorkerIdsAsync(
        IReadOnlyList<int> workerIds,
        CancellationToken cancellationToken = default);

    Task AddAsync(Vacation vacation, CancellationToken cancellationToken = default);

    Task UpdateAsync(Vacation vacation, CancellationToken cancellationToken = default);
}
