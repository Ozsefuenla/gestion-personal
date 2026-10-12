using GestionPersonal.Domain.Entities;

namespace GestionPersonal.Application.Centers;

public sealed record CenterResponse(int Id, string Name)
{
    public static CenterResponse From(Center center) => new(center.Id, center.Name);
}
