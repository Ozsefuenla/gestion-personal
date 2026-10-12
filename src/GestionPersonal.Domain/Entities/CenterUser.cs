namespace GestionPersonal.Domain.Entities;

public sealed class CenterUser
{
    public int Id { get; private set; }
    public int CenterId { get; private set; }
    public string Username { get; private set; } = default!;
    public string PasswordHash { get; private set; } = default!;

    private CenterUser()
    {
    }

    public static CenterUser Create(int id, int centerId, string username, string passwordHash)
    {
        return new CenterUser
        {
            Id = id,
            CenterId = centerId,
            Username = username.Trim(),
            PasswordHash = passwordHash
        };
    }
}
