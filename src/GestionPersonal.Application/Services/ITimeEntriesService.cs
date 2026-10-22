using GestionPersonal.Application.Common;
using GestionPersonal.Application.TimeEntries;

namespace GestionPersonal.Application.Services;

public interface ITimeEntriesService
{
    Task<Result<TodaySummaryResponse>> GetTodayAsync(
        TodaySummaryQuery query,
        CancellationToken cancellationToken = default);

    Task<Result<MonthlySummaryResponse>> GetMonthlyAsync(
        MonthlySummaryQuery query,
        CancellationToken cancellationToken = default);

    Task<Result<TimeEntryResponse>> StartAsync(
        StartTimeEntryCommand command,
        CancellationToken cancellationToken = default);

    Task<Result<TimeEntryResponse>> PauseAsync(
        PauseTimeEntryCommand command,
        CancellationToken cancellationToken = default);

    Task<Result<TimeEntryResponse>> ResumeAsync(
        ResumeTimeEntryCommand command,
        CancellationToken cancellationToken = default);

    Task<Result<TimeEntryResponse>> EndAsync(
        EndTimeEntryCommand command,
        CancellationToken cancellationToken = default);

    Task<Result<QuickClockResponse>> QuickClockAsync(
        QuickClockCommand command,
        CancellationToken cancellationToken = default);

    Task<Result<TimeEntryResponse>> AddManualAsync(
        AddManualTimeEntryCommand command,
        CancellationToken cancellationToken = default);

    Task<Result> EditActiveAsync(
        int workerId,
        EditActiveTimeEntryCommand command,
        CancellationToken cancellationToken = default);

    Task<Result> DeleteActiveAsync(int workerId, CancellationToken cancellationToken = default);

    Task<Result<TimeEntryResponse>> CloseStaleAsync(
        CloseStaleTimeEntryCommand command,
        CancellationToken cancellationToken = default);
}
