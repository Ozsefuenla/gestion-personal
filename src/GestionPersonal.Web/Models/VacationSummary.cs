namespace GestionPersonal.Web.Models;

public sealed record VacationSummary(
    Guid Id,
    int WorkerId,
    DateOnly StartDate,
    DateOnly EndDate,
    string Status,
    string Type,
    DateTimeOffset RequestedAt);
