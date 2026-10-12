using GestionPersonal.Application.Abstractions;
using GestionPersonal.Domain.Entities;
using GestionPersonal.Domain.Enums;

namespace GestionPersonal.Infrastructure.Persistence.Repositories;

public sealed class InMemoryWorkerRepository : IWorkerRepository
{
    private readonly List<Worker> _workers;

    public InMemoryWorkerRepository(IPasswordHasher passwordHasher)
    {
        _workers =
        [
            Worker.Create(1, 1, "María García", passwordHasher.Hash("1111"), Role.Worker),
            Worker.Create(2, 1, "Ana López", passwordHasher.Hash("2222"), Role.Worker),
            Worker.Create(3, 1, "Carlos Ruiz", passwordHasher.Hash("3333"), Role.Admin)
        ];
    }

    public Task<Worker?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_workers.FirstOrDefault(w => w.Id == id));
    }

    public Task<Worker?> GetByIdAndCenterAsync(int id, int centerId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_workers.FirstOrDefault(w => w.Id == id && w.CenterId == centerId));
    }

    public Task<IReadOnlyList<Worker>> GetByCenterAsync(int centerId, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Worker> result = _workers
            .Where(w => w.CenterId == centerId)
            .ToList();

        return Task.FromResult(result);
    }

    public Task<IReadOnlyList<Worker>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Worker> result = _workers.ToList();
        return Task.FromResult(result);
    }

    public void Add(Worker worker)
    {
        _workers.Add(worker);
    }
}
