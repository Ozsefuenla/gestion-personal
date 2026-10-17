using GestionPersonal.Application.Common;
using GestionPersonal.Application.TimeEntries;

namespace GestionPersonal.Application.Services;

public sealed class TimeEntriesService : ITimeEntriesService
{
    private readonly TodaySummaryQueryHandler _todayHandler;
    private readonly MonthlySummaryQueryHandler _monthlyHandler;
    private readonly StartTimeEntryCommandHandler _startHandler;
    private readonly PauseTimeEntryCommandHandler _pauseHandler;
    private readonly ResumeTimeEntryCommandHandler _resumeHandler;
    private readonly EndTimeEntryCommandHandler _endHandler;
    private readonly QuickClockCommandHandler _quickClockHandler;
    private readonly AddManualTimeEntryCommandHandler _addManualHandler;
    private readonly EditActiveTimeEntryCommandHandler _editActiveHandler;
    private readonly DeleteActiveTimeEntryCommandHandler _deleteActiveHandler;
    private readonly IRequestValidator _requestValidator;

    public TimeEntriesService(
        TodaySummaryQueryHandler todayHandler,
        MonthlySummaryQueryHandler monthlyHandler,
        StartTimeEntryCommandHandler startHandler,
        PauseTimeEntryCommandHandler pauseHandler,
        ResumeTimeEntryCommandHandler resumeHandler,
        EndTimeEntryCommandHandler endHandler,
        QuickClockCommandHandler quickClockHandler,
        AddManualTimeEntryCommandHandler addManualHandler,
        EditActiveTimeEntryCommandHandler editActiveHandler,
        DeleteActiveTimeEntryCommandHandler deleteActiveHandler,
        IRequestValidator requestValidator)
    {
        _todayHandler = todayHandler;
        _monthlyHandler = monthlyHandler;
        _startHandler = startHandler;
        _pauseHandler = pauseHandler;
        _resumeHandler = resumeHandler;
        _endHandler = endHandler;
        _quickClockHandler = quickClockHandler;
        _addManualHandler = addManualHandler;
        _editActiveHandler = editActiveHandler;
        _deleteActiveHandler = deleteActiveHandler;
        _requestValidator = requestValidator;
    }

    public async Task<Result<TodaySummaryResponse>> GetTodayAsync(
        TodaySummaryQuery query,
        CancellationToken cancellationToken = default)
    {
        var validationError = await _requestValidator.ValidateAsync(query, cancellationToken);

        if (validationError is not null)
        {
            return validationError;
        }

        return await _todayHandler.Handle(query, cancellationToken);
    }

    public async Task<Result<MonthlySummaryResponse>> GetMonthlyAsync(
        MonthlySummaryQuery query,
        CancellationToken cancellationToken = default)
    {
        var validationError = await _requestValidator.ValidateAsync(query, cancellationToken);

        if (validationError is not null)
        {
            return validationError;
        }

        return await _monthlyHandler.Handle(query, cancellationToken);
    }

    public async Task<Result<TimeEntryResponse>> StartAsync(
        StartTimeEntryCommand command,
        CancellationToken cancellationToken = default)
    {
        var validationError = await _requestValidator.ValidateAsync(command, cancellationToken);

        if (validationError is not null)
        {
            return validationError;
        }

        return await _startHandler.Handle(command, cancellationToken);
    }

    public async Task<Result<TimeEntryResponse>> PauseAsync(
        PauseTimeEntryCommand command,
        CancellationToken cancellationToken = default)
    {
        var validationError = await _requestValidator.ValidateAsync(command, cancellationToken);

        if (validationError is not null)
        {
            return validationError;
        }

        return await _pauseHandler.Handle(command, cancellationToken);
    }

    public async Task<Result<TimeEntryResponse>> ResumeAsync(
        ResumeTimeEntryCommand command,
        CancellationToken cancellationToken = default)
    {
        var validationError = await _requestValidator.ValidateAsync(command, cancellationToken);

        if (validationError is not null)
        {
            return validationError;
        }

        return await _resumeHandler.Handle(command, cancellationToken);
    }

    public async Task<Result<TimeEntryResponse>> EndAsync(
        EndTimeEntryCommand command,
        CancellationToken cancellationToken = default)
    {
        var validationError = await _requestValidator.ValidateAsync(command, cancellationToken);

        if (validationError is not null)
        {
            return validationError;
        }

        return await _endHandler.Handle(command, cancellationToken);
    }

    public async Task<Result<QuickClockResponse>> QuickClockAsync(
        QuickClockCommand command,
        CancellationToken cancellationToken = default)
    {
        var validationError = await _requestValidator.ValidateAsync(command, cancellationToken);

        if (validationError is not null)
        {
            return validationError;
        }

        return await _quickClockHandler.Handle(command, cancellationToken);
    }

    public async Task<Result<TimeEntryResponse>> AddManualAsync(
        AddManualTimeEntryCommand command,
        CancellationToken cancellationToken = default)
    {
        var validationError = await _requestValidator.ValidateAsync(command, cancellationToken);

        if (validationError is not null)
        {
            return validationError;
        }

        return await _addManualHandler.Handle(command, cancellationToken);
    }

    public async Task<Result> EditActiveAsync(
        int workerId,
        EditActiveTimeEntryCommand command,
        CancellationToken cancellationToken = default)
    {
        var validationError = await _requestValidator.ValidateAsync(command, cancellationToken);

        if (validationError is not null)
        {
            return validationError;
        }

        return await _editActiveHandler.Handle(workerId, command, cancellationToken);
    }

    public Task<Result> DeleteActiveAsync(int workerId, CancellationToken cancellationToken = default) =>
        _deleteActiveHandler.Handle(workerId, cancellationToken);
}
