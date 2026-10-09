using GestionPersonal.Application.Abstractions;
using GestionPersonal.Application.TimeEntries;
using GestionPersonal.Domain.Entities;

namespace GestionPersonal.Application.Workers;

internal static class WorkerStatusBuilder
{
    public static async Task<WorkerStatusResponse> BuildAsync(
        Worker worker,
        DateTimeOffset now,
        DateTimeOffset from,
        DateTimeOffset to,
        ITimeEntryRepository timeEntryRepository,
        CancellationToken cancellationToken)
    {
        var openEntry = await timeEntryRepository.GetOpenEntryByWorkerAsync(worker.Id, cancellationToken);
        var entries = await timeEntryRepository.GetByWorkerBetweenAsync(worker.Id, from, to, cancellationToken);

        return new WorkerStatusResponse(
            worker.Id,
            worker.FullName,
            worker.Role.ToString().ToLowerInvariant(),
            TodayStatusHelper.GetCurrentStatus(openEntry, entries),
            openEntry?.StartedAt,
            openEntry?.CurrentPausedAt,
            entries.Aggregate(TimeSpan.Zero, (total, e) => total + e.GetWorkedDuration(now)),
            entries.Aggregate(TimeSpan.Zero, (total, e) => total + e.GetPausedDuration(now)),
            TodayStatusHelper.BuildTimeline(entries, now));
    }
}
