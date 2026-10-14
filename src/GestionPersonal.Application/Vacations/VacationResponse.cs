using GestionPersonal.Domain.Entities;

namespace GestionPersonal.Application.Vacations;

public sealed record VacationResponse(
    Guid Id,
    int WorkerId,
    DateOnly StartDate,
    DateOnly EndDate,
    string Status,
    string Type,
    DateTimeOffset RequestedAt)
{
    public static VacationResponse From(Vacation vacation) => new(
        vacation.Id,
        vacation.WorkerId,
        vacation.StartDate,
        vacation.EndDate,
        vacation.Status.ToString().ToLowerInvariant(),
        VacationTypeHelper.Format(vacation.Type),
        vacation.RequestedAt);
}
