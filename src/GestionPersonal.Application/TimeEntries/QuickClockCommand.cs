namespace GestionPersonal.Application.TimeEntries;

public sealed record QuickClockCommand(
    int WorkerId,
    string Pin,
    string Action,
    DateOnly? EndDate = null,
    TimeOnly? EndTime = null);
