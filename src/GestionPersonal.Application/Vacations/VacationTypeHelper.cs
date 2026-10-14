using GestionPersonal.Domain.Enums;

namespace GestionPersonal.Application.Vacations;

internal static class VacationTypeHelper
{
    public static VacationType? Parse(string? type) =>
        type?.Trim().ToLowerInvariant() switch
        {
            "vacations" => VacationType.Vacations,
            "sick-leave" => VacationType.SickLeave,
            "other" => VacationType.Other,
            _ => null
        };

    public static string Format(VacationType type) =>
        type switch
        {
            VacationType.Vacations => "vacations",
            VacationType.SickLeave => "sick-leave",
            _ => "other"
        };
}
