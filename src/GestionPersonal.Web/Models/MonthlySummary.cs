namespace GestionPersonal.Web.Models;

public sealed record MonthlySummary(
    int Year,
    int Month,
    TimeSpan DailyTarget,
    IReadOnlyList<DailySummary> Days);

public sealed record DailySummary(
    int Day,
    TimeSpan WorkedTime,
    bool HasEntries);
