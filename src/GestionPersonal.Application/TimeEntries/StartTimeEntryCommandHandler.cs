using GestionPersonal.Application.Abstractions;
using GestionPersonal.Application.Common;
using GestionPersonal.Domain.Entities;

namespace GestionPersonal.Application.TimeEntries;

public sealed class StartTimeEntryCommandHandler
{
    private readonly IWorkerRepository _workerRepository;
    private readonly ICurrentUser _currentUser;
    private readonly ITimeEntryRepository _timeEntryRepository;

    public StartTimeEntryCommandHandler(
        IWorkerRepository workerRepository,
        ICurrentUser currentUser,
        ITimeEntryRepository timeEntryRepository)
    {
        _workerRepository = workerRepository;
        _currentUser = currentUser;
        _timeEntryRepository = timeEntryRepository;
    }

    public async Task<Result<TimeEntryResponse>> Handle(
        StartTimeEntryCommand command,
        CancellationToken cancellationToken = default)
    {
        var workerResult = await WorkerScope.GetSelfScopedWorkerAsync(
            _workerRepository, _currentUser, command.WorkerId, cancellationToken);

        if (workerResult.IsFailure)
        {
            return workerResult.Error;
        }

        return await StartForWorkerAsync(command.WorkerId, cancellationToken);
    }

    internal async Task<Result<TimeEntryResponse>> StartForWorkerAsync(
        int workerId,
        CancellationToken cancellationToken = default)
    {
        var existing = await _timeEntryRepository.GetOpenEntryByWorkerAsync(workerId, cancellationToken);

        if (existing is not null)
        {
            return Error.Conflict("La jornada ya está iniciada.");
        }

        var timeEntry = TimeEntry.Start(workerId, DateTimeOffset.UtcNow);

        await _timeEntryRepository.AddAsync(timeEntry, cancellationToken);

        return TimeEntryResponse.From(timeEntry);
    }
}
