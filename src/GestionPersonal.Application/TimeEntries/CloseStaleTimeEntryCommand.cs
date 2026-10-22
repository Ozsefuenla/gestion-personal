namespace GestionPersonal.Application.TimeEntries;

public sealed record CloseStaleTimeEntryCommand(int WorkerId, DateOnly Date, TimeOnly EndTime);
