using GestionPersonal.Application.Abstractions;
using GestionPersonal.Domain.Entities;
using GestionPersonal.Domain.Enums;

namespace GestionPersonal.TestSupport.Fakes;

public sealed class InMemoryCenterUserRepository : ICenterUserRepository
{
    private readonly List<CenterUser> _users;
    private readonly Lock _gate = new();

    public InMemoryCenterUserRepository(IPasswordHasher passwordHasher)
    {
        _users =
        [
            CenterUser.Create(1, null, "superadmin", passwordHasher.Hash("superadmin"), UserRole.SuperAdmin),
            CenterUser.Create(2, 1, "carlos.ruiz", passwordHasher.Hash("sol"), UserRole.CenterAdmin),
            CenterUser.Create(3, 1, "centro", passwordHasher.Hash("centro"), UserRole.Center)
        ];
    }

    public Task<CenterUser?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        lock (_gate)
        {
            return Task.FromResult(_users.FirstOrDefault(u => u.Username == username));
        }
    }

    public Task<CenterUser?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        lock (_gate)
        {
            return Task.FromResult(_users.FirstOrDefault(u => u.Id == id));
        }
    }

    public Task<IReadOnlyList<CenterUser>> GetByCenterAsync(int centerId, CancellationToken cancellationToken = default)
    {
        lock (_gate)
        {
            IReadOnlyList<CenterUser> result = _users
                .Where(u => u.CenterId == centerId)
                .OrderBy(u => u.Username)
                .ToList();

            return Task.FromResult(result);
        }
    }

    public Task<CenterUser> AddAsync(CenterUser user, CancellationToken cancellationToken = default)
    {
        lock (_gate)
        {
            var id = _users.Count == 0 ? 1 : _users.Max(u => u.Id) + 1;

            var persisted = CenterUser.Rehydrate(
                id,
                user.CenterId,
                user.Username,
                user.PasswordHash,
                user.Role,
                user.Active,
                user.MustChangePassword);

            _users.Add(persisted);

            return Task.FromResult(persisted);
        }
    }

    public Task UpdateAsync(CenterUser user, CancellationToken cancellationToken = default)
    {
        lock (_gate)
        {
            var index = _users.FindIndex(u => u.Id == user.Id);

            if (index >= 0)
            {
                _users[index] = user;
            }

            return Task.CompletedTask;
        }
    }
}
