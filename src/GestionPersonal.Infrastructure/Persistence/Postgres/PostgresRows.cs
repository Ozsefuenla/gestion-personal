namespace GestionPersonal.Infrastructure.Persistence.Postgres;

internal sealed record CenterRow(int Id, string Name, bool Active);

internal sealed record CenterUserRow(
    int Id,
    int? CenterId,
    string Username,
    string PasswordHash,
    string Role,
    bool Active,
    bool MustChangePassword);

internal sealed record WorkerRow(
    int Id,
    int CenterId,
    string FullName,
    string PinHash,
    bool Active,
    bool MustChangePin,
    TimeSpan DailyHours,
    DateTime CreatedAt);

internal sealed record TimeEntryRow(
    Guid Id,
    int WorkerId,
    string Status,
    DateTime StartedAt,
    DateTime? EndedAt);

internal sealed record PauseRow(Guid TimeEntryId, DateTime PausedAt, DateTime? ResumedAt);

internal sealed record VacationRow(
    Guid Id,
    int WorkerId,
    DateOnly StartDate,
    DateOnly EndDate,
    string Type,
    string Status,
    DateTime RequestedAt,
    DateTime? DecidedAt);

internal sealed record MonitoringOverviewRow(
    int TotalCenters,
    int ActiveCenters,
    int ActiveWorkers,
    int ClockInsToday,
    int PendingAbsences,
    int ExceptionsLast24h,
    int RequestsLast24h,
    int ErrorsLast24h);

internal sealed record SeverityCountRow(string Severity, int Count);

internal sealed record ExceptionListRow(
    long Id,
    DateTime OccurredAt,
    string ExceptionType,
    string Message,
    string? RequestPath,
    string? HttpMethod,
    int? StatusCode,
    string? ActorRole,
    int? CenterId);

internal sealed record ErrorRateRow(DateTime Hour, int Requests, int Errors);
