using GestionPersonal.Application.Abstractions;
using GestionPersonal.Domain.Entities;
using GestionPersonal.Domain.Enums;

namespace GestionPersonal.Infrastructure.Persistence.Repositories;

public sealed class InMemoryWorkerRepository : IWorkerRepository
{
    private readonly List<Worker> _workers =
    [
        Worker.Create("María García", "1234", Role.Worker),
        Worker.Create("Ana López", "0000", Role.Worker),
        Worker.Create("Carlos Ruiz", "9999", Role.Admin)
    ];

    public Task<Worker?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_workers.FirstOrDefault(w => w.Id == id));
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
