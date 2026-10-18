using GestionPersonal.Application.Abstractions;
using GestionPersonal.Domain.Entities;

namespace GestionPersonal.TestSupport.Fakes;

public sealed class InMemoryCenterUserRepository : ICenterUserRepository
{
    private readonly List<CenterUser> _users;

    public InMemoryCenterUserRepository(IPasswordHasher passwordHasher)
    {
        _users =
        [
            CenterUser.Create(1, 1, "admin", passwordHasher.Hash("admin"))
        ];
    }

    public Task<CenterUser?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_users.FirstOrDefault(u => u.Username == username));
    }
}
