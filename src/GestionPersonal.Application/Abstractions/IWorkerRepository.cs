using GestionPersonal.Domain.Entities;

namespace GestionPersonal.Application.Abstractions;

public interface IWorkerRepository
{
    Task<Worker?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<Worker?> GetByIdAndCenterAsync(int id, int centerId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Worker>> GetByCenterAsync(int centerId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Worker>> GetAllAsync(CancellationToken cancellationToken = default);

    void Add(Worker worker);
}
