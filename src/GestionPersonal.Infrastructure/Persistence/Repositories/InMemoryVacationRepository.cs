using GestionPersonal.Application.Abstractions;
using GestionPersonal.Domain.Entities;
using GestionPersonal.Domain.Enums;

namespace GestionPersonal.Infrastructure.Persistence.Repositories;

public sealed class InMemoryVacationRepository : IVacationRepository
{
    private readonly List<Vacation> _vacations = [];
    private readonly Lock _gate = new();

    public Task<IReadOnlyList<Vacation>> GetByWorkerAsync(int workerId, CancellationToken cancellationToken = default)
    {
        lock (_gate)
        {
            IReadOnlyList<Vacation> result = _vacations
                .Where(v => v.WorkerId == workerId && v.Status != VacationStatus.Deleted)
                .OrderByDescending(v => v.StartDate)
                .ToList();

            return Task.FromResult(result);
        }
    }

    public Task<Vacation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        lock (_gate)
        {
            return Task.FromResult(_vacations.FirstOrDefault(v => v.Id == id));
        }
    }

    public Task<IReadOnlyList<Vacation>> GetApprovedByWorkerBetweenAsync(
        int workerId,
        DateOnly from,
        DateOnly to,
        CancellationToken cancellationToken = default)
    {
        lock (_gate)
        {
            IReadOnlyList<Vacation> result = _vacations
                .Where(v => v.WorkerId == workerId &&
                            v.Status == VacationStatus.Approved &&
                            v.StartDate <= to &&
                            v.EndDate >= from)
                .OrderBy(v => v.StartDate)
                .ToList();

            return Task.FromResult(result);
        }
    }

    public Task<bool> HasOverlappingActiveAsync(
        int workerId,
        DateOnly startDate,
        DateOnly endDate,
        CancellationToken cancellationToken = default)
    {
        lock (_gate)
        {
            var overlaps = _vacations.Any(v =>
                v.WorkerId == workerId &&
                v.Status != VacationStatus.Deleted &&
                v.Overlaps(startDate, endDate));

            return Task.FromResult(overlaps);
        }
    }

    public Task<IReadOnlyList<Vacation>> GetByWorkerIdsAsync(
        IReadOnlyList<int> workerIds,
        CancellationToken cancellationToken = default)
    {
        lock (_gate)
        {
            IReadOnlyList<Vacation> result = _vacations
                .Where(v => workerIds.Contains(v.WorkerId) && v.Status != VacationStatus.Deleted)
                .OrderByDescending(v => v.StartDate)
                .ToList();

            return Task.FromResult(result);
        }
    }

    public Task AddAsync(Vacation vacation, CancellationToken cancellationToken = default)
    {
        lock (_gate)
        {
            _vacations.Add(vacation);
            return Task.CompletedTask;
        }
    }

    public Task UpdateAsync(Vacation vacation, CancellationToken cancellationToken = default)
    {
        lock (_gate)
        {
            var index = _vacations.FindIndex(v => v.Id == vacation.Id);

            if (index >= 0)
            {
                _vacations[index] = vacation;
            }

            return Task.CompletedTask;
        }
    }
}
