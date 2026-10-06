using GestionPersonal.Domain.Entities;

namespace GestionPersonal.Application.Abstractions;

public interface IWorkerRepository
{
    Task<Worker?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Worker>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default);

    void Add(Worker worker);
}
