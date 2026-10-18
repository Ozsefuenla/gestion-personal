namespace GestionPersonal.Domain.Entities;

public sealed class Worker
{
    public int Id { get; private set; }
    public int CenterId { get; private set; }
    public string FullName { get; private set; } = default!;
    public string PinHash { get; private set; } = default!;
    public bool Active { get; private set; }
    public bool MustChangePin { get; private set; }
    public TimeSpan DailyHours { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    private Worker()
    {
    }

    public static Worker Create(int centerId, string fullName, string pinHash, TimeSpan? dailyHours = null, bool mustChangePin = false)
    {
        return new Worker
        {
            CenterId = centerId,
            FullName = fullName.Trim(),
            PinHash = pinHash,
            Active = true,
            MustChangePin = mustChangePin,
            DailyHours = dailyHours ?? TimeSpan.FromHours(8),
            CreatedAt = DateTimeOffset.UtcNow
        };
    }

    public static Worker Rehydrate(
        int id,
        int centerId,
        string fullName,
        string pinHash,
        bool active,
        bool mustChangePin,
        TimeSpan dailyHours,
        DateTimeOffset createdAt)
    {
        return new Worker
        {
            Id = id,
            CenterId = centerId,
            FullName = fullName,
            PinHash = pinHash,
            Active = active,
            MustChangePin = mustChangePin,
            DailyHours = dailyHours,
            CreatedAt = createdAt
        };
    }

    public void ChangePin(string newPinHash)
    {
        PinHash = newPinHash;
        MustChangePin = false;
    }

    public void ResetPin(string newPinHash)
    {
        PinHash = newPinHash;
        MustChangePin = true;
    }

    public void Rename(string fullName)
    {
        FullName = fullName.Trim();
    }

    public void Activate()
    {
        Active = true;
    }

    public void Deactivate()
    {
        Active = false;
    }
}
