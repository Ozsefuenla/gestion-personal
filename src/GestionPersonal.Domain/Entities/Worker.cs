using GestionPersonal.Domain.Enums;

namespace GestionPersonal.Domain.Entities;

public sealed class Worker
{
    public Guid Id { get; private set; }
    public string FullName { get; private set; } = default!;
    public string Email { get; private set; } = default!;
    public Role Role { get; private set; }
    public bool Active { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    private Worker()
    {
    }

    public static Worker Create(string fullName, string email, Role role)
    {
        return new Worker
        {
            Id = Guid.NewGuid(),
            FullName = fullName.Trim(),
            Email = email.Trim().ToLowerInvariant(),
            Role = role,
            Active = true,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }
}
