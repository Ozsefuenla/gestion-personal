using GestionPersonal.Application.Abstractions;
using GestionPersonal.Application.Common;
using GestionPersonal.Domain.Entities;
using GestionPersonal.Domain.Enums;

namespace GestionPersonal.Application.Platform;

internal static class CenterUserAccess
{
    public static Error? EnsureCanManage(ICurrentUser currentUser, CenterUser target)
    {
        if (target.Role == UserRole.SuperAdmin)
        {
            return Error.Forbidden("No se puede modificar a un SuperAdmin.");
        }

        if (currentUser.Role == UserRole.SuperAdmin)
        {
            return null;
        }

        if (AccessControl.RequireCenterAdmin(currentUser) is { } error)
        {
            return error;
        }

        if (target.Role != UserRole.Center || target.CenterId != currentUser.CenterId)
        {
            return Error.Forbidden("No autorizado para este usuario.");
        }

        return null;
    }
}
