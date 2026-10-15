namespace GestionPersonal.Application.TimeEntries;

public sealed record AddManualTimeEntryCommand(
    int WorkerId,
    string? Type,
    DateOnly? Date,
    TimeOnly? StartTime,
    TimeOnly? EndTime);
