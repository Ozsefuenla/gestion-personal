using GestionPersonal.Domain.Enums;

namespace GestionPersonal.Application.Centers;

public sealed record CenterSessionResponse(
    int UserId,
    int? CenterId,
    string CenterName,
    string Username,
    UserRole Role,
    bool MustChangePassword);
