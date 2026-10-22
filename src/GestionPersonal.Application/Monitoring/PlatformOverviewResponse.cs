namespace GestionPersonal.Application.Monitoring;

public sealed record PlatformOverviewResponse(
    int TotalCenters,
    int ActiveCenters,
    int ActiveWorkers,
    int ClockInsToday,
    int PendingAbsences,
    int ExceptionsLast24h,
    int RequestsLast24h,
    int ErrorsLast24h,
    IReadOnlyList<SeverityCount> SeverityBreakdown);
