using GestionPersonal.Application.Abstractions;
using GestionPersonal.Domain.Entities;

namespace GestionPersonal.Infrastructure.Persistence.Repositories;

public sealed class InMemoryCenterRepository : ICenterRepository
{
    private readonly List<Center> _centers =
    [
        Center.Create(1, "Peluquería Sol")
    ];

    public Task<Center?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_centers.FirstOrDefault(c => c.Id == id));
    }

    public Task<IReadOnlyList<Center>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Center> result = _centers.ToList();
        return Task.FromResult(result);
    }
}
