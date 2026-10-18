using GestionPersonal.Domain.Entities;

namespace GestionPersonal.Application.Abstractions;

public interface ICenterUserRepository
{
    Task<CenterUser?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);

    Task<CenterUser?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CenterUser>> GetByCenterAsync(int centerId, CancellationToken cancellationToken = default);

    Task<CenterUser> AddAsync(CenterUser user, CancellationToken cancellationToken = default);

    Task UpdateAsync(CenterUser user, CancellationToken cancellationToken = default);
}
