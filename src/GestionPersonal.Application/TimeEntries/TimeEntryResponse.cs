using GestionPersonal.Domain.Entities;
using GestionPersonal.Domain.Enums;

namespace GestionPersonal.Application.TimeEntries;

public sealed record TimeEntryResponse(
    Guid Id,
    Guid WorkerId,
    string Status,
    DateTimeOffset StartedAt,
    DateTimeOffset? EndedAt,
    IReadOnlyList<PauseResponse> Pauses)
{
    public static TimeEntryResponse From(TimeEntry entry) =>
        new(
            entry.Id,
            entry.WorkerId,
            ToStatus(entry.Status),
            entry.StartedAt,
            entry.EndedAt,
            entry.Pauses.Select(p => new PauseResponse(p.PausedAt, p.ResumedAt)).ToList());

    private static string ToStatus(TimeEntryStatus status) => status switch
    {
        TimeEntryStatus.InProgress => "in-progress",
        TimeEntryStatus.Paused => "paused",
        TimeEntryStatus.Finished => "finished",
        _ => status.ToString().ToLowerInvariant()
    };
}

public sealed record PauseResponse(DateTimeOffset PausedAt, DateTimeOffset? ResumedAt);
