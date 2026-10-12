using GestionPersonal.Application.Abstractions;
using GestionPersonal.Domain.Entities;

namespace GestionPersonal.Infrastructure.Persistence.Repositories;

public sealed class InMemoryCenterUserRepository : ICenterUserRepository
{
    private readonly List<CenterUser> _users =
    [
        CenterUser.Create(1, 1, "admin", "admin")
    ];

    public Task<CenterUser?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_users.FirstOrDefault(u => u.Username == username));
    }
}
