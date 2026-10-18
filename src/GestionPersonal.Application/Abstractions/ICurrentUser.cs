using GestionPersonal.Domain.Enums;

namespace GestionPersonal.Application.Abstractions;

public interface ICurrentUser
{
    bool IsAuthenticated { get; }

    int? SubjectId { get; }

    int? CenterId { get; }

    UserRole? Role { get; }

    int? WorkerId { get; }

    bool MustChangePassword { get; }
}
