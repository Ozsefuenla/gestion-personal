namespace GestionPersonal.Application.TimeEntries;

public sealed record MonthlySummaryQuery(Guid WorkerId, int Year, int Month);
