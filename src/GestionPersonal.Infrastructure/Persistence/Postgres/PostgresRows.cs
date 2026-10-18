namespace GestionPersonal.Infrastructure.Persistence.Postgres;

internal sealed record CenterRow(int Id, string Name);

internal sealed record CenterUserRow(int Id, int CenterId, string Username, string PasswordHash);

internal sealed record WorkerRow(
    int Id,
    int CenterId,
    string FullName,
    string PinHash,
    string Role,
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
