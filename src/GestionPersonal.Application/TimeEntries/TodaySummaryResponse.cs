namespace GestionPersonal.Application.TimeEntries;

public sealed record TodaySummaryResponse(
    string CurrentStatus,
    DateTimeOffset? StartedAt,
    DateTimeOffset? CurrentPausedAt,
    TimeSpan WorkedTime,
    TimeSpan PausedTime,
    int PauseCount,
    int CompletedEntries,
    IReadOnlyList<TimelineSegmentResponse> Timeline);

public sealed record TimelineSegmentResponse(string Type, DateTimeOffset From, DateTimeOffset To);
