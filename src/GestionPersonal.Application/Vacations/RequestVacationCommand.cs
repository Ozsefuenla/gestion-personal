namespace GestionPersonal.Application.Vacations;

public sealed record RequestVacationCommand(
    int WorkerId,
    DateOnly StartDate,
    DateOnly EndDate,
    string Type);
