using GestionPersonal.Domain.Enums;

namespace GestionPersonal.Domain.Entities;

public sealed class Worker
{
    public Guid Id { get; private set; }
    public string FullName { get; private set; } = default!;
    public string PinHash { get; private set; } = default!;
    public Role Role { get; private set; }
    public bool Active { get; private set; }
    public TimeSpan DailyHours { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    private Worker()
    {
    }

    public static Worker Create(string fullName, string pinHash, Role role, TimeSpan? dailyHours = null)
    {
        return new Worker
        {
            Id = Guid.NewGuid(),
            FullName = fullName.Trim(),
            PinHash = pinHash,
            Role = role,
            Active = true,
            DailyHours = dailyHours ?? TimeSpan.FromHours(8),
            CreatedAt = DateTimeOffset.UtcNow
        };
    }

    public void ChangePin(string newPinHash)
    {
        PinHash = newPinHash;
    }
}
