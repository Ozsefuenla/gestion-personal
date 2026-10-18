using GestionPersonal.Application.Abstractions;
using GestionPersonal.Domain.Enums;

namespace GestionPersonal.Application.Common;

public static class AccessControl
{
    public static Error? RequireSuperAdmin(ICurrentUser user) =>
        user.Role == UserRole.SuperAdmin
            ? null
            : Error.Forbidden("Se requiere ser SuperAdmin.");

    public static Error? RequireCenterAdmin(ICurrentUser user) =>
        user.Role == UserRole.CenterAdmin && user.CenterId is not null
            ? null
            : Error.Forbidden("Se requiere ser administrador del centro.");

    public static Error? RequireCenterMember(ICurrentUser user) =>
        user.CenterId is not null && user.Role is UserRole.Center or UserRole.CenterAdmin or UserRole.CenterWorker
            ? null
            : Error.Forbidden("No autenticado.");

    public static Error? RequireKiosk(ICurrentUser user) =>
        user.CenterId is not null && user.Role is UserRole.Center or UserRole.CenterAdmin
            ? null
            : Error.Forbidden("No autorizado.");

    public static Error? RequireSelfOrAdmin(ICurrentUser user, int workerId)
    {
        if (user.CenterId is null)
        {
            return Error.Forbidden("No autenticado.");
        }

        if (user.Role == UserRole.CenterAdmin)
        {
            return null;
        }

        if (user.Role == UserRole.CenterWorker && user.WorkerId == workerId)
        {
            return null;
        }

        return Error.Forbidden("No autorizado.");
    }
}
