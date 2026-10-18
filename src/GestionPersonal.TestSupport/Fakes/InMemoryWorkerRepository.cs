using GestionPersonal.Application.Abstractions;
using GestionPersonal.Domain.Entities;

namespace GestionPersonal.TestSupport.Fakes;

public sealed class InMemoryWorkerRepository : IWorkerRepository
{
    private readonly List<Worker> _workers;
    private readonly Lock _gate = new();

    public InMemoryWorkerRepository(IPasswordHasher passwordHasher)
    {
        var now = DateTimeOffset.UtcNow;

        _workers =
        [
            Worker.Rehydrate(1, 1, "María García", passwordHasher.Hash("1111"), true, false, TimeSpan.FromHours(8), now),
            Worker.Rehydrate(2, 1, "Ana López", passwordHasher.Hash("2222"), true, false, TimeSpan.FromHours(8), now),
            Worker.Rehydrate(3, 1, "Carlos Ruiz", passwordHasher.Hash("3333"), true, false, TimeSpan.FromHours(8), now)
        ];
    }

    public Task<Worker?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        lock (_gate)
        {
            return Task.FromResult(_workers.FirstOrDefault(w => w.Id == id));
        }
    }

    public Task<Worker?> GetByIdAndCenterAsync(int id, int centerId, CancellationToken cancellationToken = default)
    {
        lock (_gate)
        {
            return Task.FromResult(_workers.FirstOrDefault(w => w.Id == id && w.CenterId == centerId));
        }
    }

    public Task<IReadOnlyList<Worker>> GetByCenterAsync(int centerId, CancellationToken cancellationToken = default)
    {
        lock (_gate)
        {
            IReadOnlyList<Worker> result = _workers
                .Where(w => w.CenterId == centerId)
                .ToList();

            return Task.FromResult(result);
        }
    }

    public Task<IReadOnlyList<Worker>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        lock (_gate)
        {
            IReadOnlyList<Worker> result = _workers.ToList();
            return Task.FromResult(result);
        }
    }

    public Task<Worker> AddAsync(Worker worker, CancellationToken cancellationToken = default)
    {
        lock (_gate)
        {
            var id = _workers.Count == 0 ? 1 : _workers.Max(w => w.Id) + 1;

            var persisted = Worker.Rehydrate(
                id,
                worker.CenterId,
                worker.FullName,
                worker.PinHash,
                worker.Active,
                worker.MustChangePin,
                worker.DailyHours,
                worker.CreatedAt);

            _workers.Add(persisted);

            return Task.FromResult(persisted);
        }
    }

    public Task UpdateAsync(Worker worker, CancellationToken cancellationToken = default)
    {
        lock (_gate)
        {
            var index = _workers.FindIndex(w => w.Id == worker.Id);

            if (index >= 0)
            {
                _workers[index] = worker;
            }

            return Task.CompletedTask;
        }
    }
}
