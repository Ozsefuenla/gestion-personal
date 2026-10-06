using GestionPersonal.Application.Abstractions;
using GestionPersonal.Domain.Entities;
using GestionPersonal.Domain.Enums;

namespace GestionPersonal.Infrastructure.Persistence.Repositories;

public sealed class InMemoryWorkerRepository : IWorkerRepository
{
    private readonly List<Worker> _workers =
    [
        Worker.Create("María García", "maria@example.com", Role.Worker),
        Worker.Create("Ana López", "ana@example.com", Role.Worker),
        Worker.Create("Carlos Ruiz", "carlos@example.com", Role.Admin)
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

    public Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_workers.Any(w => w.Email == email));
    }

    public void Add(Worker worker)
    {
        _workers.Add(worker);
    }
}
