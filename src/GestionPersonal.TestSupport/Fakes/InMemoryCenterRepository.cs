using GestionPersonal.Application.Abstractions;
using GestionPersonal.Domain.Entities;

namespace GestionPersonal.TestSupport.Fakes;

public sealed class InMemoryCenterRepository : ICenterRepository
{
    private readonly List<Center> _centers =
    [
        Center.Create(1, "Peluquería Sol")
    ];

    private readonly Lock _gate = new();

    public Task<Center?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        lock (_gate)
        {
            return Task.FromResult(_centers.FirstOrDefault(c => c.Id == id));
        }
    }

    public Task<IReadOnlyList<Center>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        lock (_gate)
        {
            IReadOnlyList<Center> result = _centers.ToList();
            return Task.FromResult(result);
        }
    }

    public Task<Center> AddAsync(Center center, CancellationToken cancellationToken = default)
    {
        lock (_gate)
        {
            var id = _centers.Count == 0 ? 1 : _centers.Max(c => c.Id) + 1;

            var persisted = Center.Rehydrate(id, center.Name, center.Active);
            _centers.Add(persisted);

            return Task.FromResult(persisted);
        }
    }

    public Task UpdateAsync(Center center, CancellationToken cancellationToken = default)
    {
        lock (_gate)
        {
            var index = _centers.FindIndex(c => c.Id == center.Id);

            if (index >= 0)
            {
                _centers[index] = center;
            }

            return Task.CompletedTask;
        }
    }
}
