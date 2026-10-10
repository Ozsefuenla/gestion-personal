namespace GestionPersonal.Application.TimeEntries;

public sealed record MonthlySummaryResponse(
    int Year,
    int Month,
    TimeSpan DailyTarget,
    IReadOnlyList<DailySummaryResponse> Days);

public sealed record DailySummaryResponse(
    int Day,
    TimeSpan WorkedTime,
    bool HasEntries);
