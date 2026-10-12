namespace GestionPersonal.Domain.Entities;

public sealed class Center
{
    public int Id { get; private set; }
    public string Name { get; private set; } = default!;

    private Center()
    {
    }

    public static Center Create(int id, string name)
    {
        return new Center
        {
            Id = id,
            Name = name.Trim()
        };
    }
}
