namespace GestionPersonal.Domain.Entities;

public sealed class Center
{
    public int Id { get; private set; }
    public string Name { get; private set; } = default!;
    public bool Active { get; private set; }

    private Center()
    {
    }

    public static Center Create(int id, string name, bool active = true)
    {
        return new Center
        {
            Id = id,
            Name = name.Trim(),
            Active = active
        };
    }

    public static Center Rehydrate(int id, string name, bool active)
    {
        return new Center
        {
            Id = id,
            Name = name,
            Active = active
        };
    }

    public void Rename(string name)
    {
        Name = name.Trim();
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
