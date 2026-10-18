using GestionPersonal.Domain.Entities;

namespace GestionPersonal.Application.Abstractions;

public interface ICenterRepository
{
    Task<Center?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Center>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<Center> AddAsync(Center center, CancellationToken cancellationToken = default);

    Task UpdateAsync(Center center, CancellationToken cancellationToken = default);
}
