using GestionPersonal.Application.TimeEntries;

namespace GestionPersonal.Application.Workers;

public sealed record WorkerStatusResponse(
    Guid Id,
    string FullName,
    string Role,
    string CurrentStatus,
    DateTimeOffset? StartedAt,
    DateTimeOffset? CurrentPausedAt,
    TimeSpan WorkedTime,
    TimeSpan PausedTime,
    IReadOnlyList<TimelineSegmentResponse> Timeline);
