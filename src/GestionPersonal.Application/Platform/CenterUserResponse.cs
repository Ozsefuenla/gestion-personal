using GestionPersonal.Domain.Entities;
using GestionPersonal.Domain.Enums;

namespace GestionPersonal.Application.Platform;

public sealed record CenterUserResponse(int Id, int? CenterId, string Username, UserRole Role, bool Active)
{
    public static CenterUserResponse From(CenterUser user) =>
        new(user.Id, user.CenterId, user.Username, user.Role, user.Active);
}
