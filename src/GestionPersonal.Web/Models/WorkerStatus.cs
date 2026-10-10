namespace GestionPersonal.Web.Models;

public sealed record WorkerStatus(
    Guid Id,
    string FullName,
    string Role,
    string CurrentStatus,
    DateTimeOffset? StartedAt,
    DateTimeOffset? CurrentPausedAt,
    TimeSpan WorkedTime,
    TimeSpan PausedTime,
    TimeSpan DailyHours,
    IReadOnlyList<TimelineSegment> Timeline);

public sealed record TimelineSegment(string Type, DateTimeOffset From, DateTimeOffset To);
