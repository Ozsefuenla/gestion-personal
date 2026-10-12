using GestionPersonal.Domain.Entities;

namespace GestionPersonal.Application.Abstractions;

public interface ICenterUserRepository
{
    Task<CenterUser?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);
}
