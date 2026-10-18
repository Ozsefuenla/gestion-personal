using GestionPersonal.Domain.Enums;

namespace GestionPersonal.Domain.Entities;

public sealed class CenterUser
{
    public int Id { get; private set; }
    public int? CenterId { get; private set; }
    public string Username { get; private set; } = default!;
    public string PasswordHash { get; private set; } = default!;
    public UserRole Role { get; private set; }
    public bool Active { get; private set; }
    public bool MustChangePassword { get; private set; }

    private CenterUser()
    {
    }

    public static CenterUser Create(
        int id,
        int? centerId,
        string username,
        string passwordHash,
        UserRole role,
        bool active = true,
        bool mustChangePassword = false)
    {
        return new CenterUser
        {
            Id = id,
            CenterId = centerId,
            Username = username.Trim(),
            PasswordHash = passwordHash,
            Role = role,
            Active = active,
            MustChangePassword = mustChangePassword
        };
    }

    public static CenterUser Rehydrate(
        int id,
        int? centerId,
        string username,
        string passwordHash,
        UserRole role,
        bool active,
        bool mustChangePassword)
    {
        return new CenterUser
        {
            Id = id,
            CenterId = centerId,
            Username = username,
            PasswordHash = passwordHash,
            Role = role,
            Active = active,
            MustChangePassword = mustChangePassword
        };
    }

    public void Deactivate()
    {
        Active = false;
    }

    public void Activate()
    {
        Active = true;
    }

    public void ResetPassword(string passwordHash)
    {
        PasswordHash = passwordHash;
        MustChangePassword = true;
    }

    public void ChangePassword(string passwordHash)
    {
        PasswordHash = passwordHash;
        MustChangePassword = false;
    }
}
