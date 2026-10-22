using GestionPersonal.Application.Abstractions;
using GestionPersonal.Application.Common;

namespace GestionPersonal.Application.TimeEntries;

public sealed class EndTimeEntryCommandHandler
{
    private readonly IWorkerRepository _workerRepository;
    private readonly ICurrentUser _currentUser;
    private readonly ITimeEntryRepository _timeEntryRepository;

    public EndTimeEntryCommandHandler(
        IWorkerRepository workerRepository,
        ICurrentUser currentUser,
        ITimeEntryRepository timeEntryRepository)
    {
        _workerRepository = workerRepository;
        _currentUser = currentUser;
        _timeEntryRepository = timeEntryRepository;
    }

    public async Task<Result<TimeEntryResponse>> Handle(
        EndTimeEntryCommand command,
        CancellationToken cancellationToken = default)
    {
        var workerResult = await WorkerScope.GetSelfScopedWorkerAsync(
            _workerRepository, _currentUser, command.WorkerId, cancellationToken);

        if (workerResult.IsFailure)
        {
            return workerResult.Error;
        }

        return await EndForWorkerAsync(command.WorkerId, cancellationToken);
    }

    internal async Task<Result<TimeEntryResponse>> EndForWorkerAsync(
        int workerId,
        CancellationToken cancellationToken = default)
    {
        var existing = await _timeEntryRepository.GetOpenEntryByWorkerAsync(workerId, cancellationToken);

        if (existing is null)
        {
            return Error.Conflict("No hay una jornada abierta que finalizar.");
        }

        existing.End(DateTimeOffset.UtcNow);

        await _timeEntryRepository.UpdateAsync(existing, cancellationToken);

        return TimeEntryResponse.From(existing);
    }

    internal async Task<Result<TimeEntryResponse>> EndForWorkerAtAsync(
        int workerId,
        DateTimeOffset endAt,
        CancellationToken cancellationToken = default)
    {
        var existing = await _timeEntryRepository.GetOpenEntryByWorkerAsync(workerId, cancellationToken);

        if (existing is null)
        {
            return Error.Conflict("No hay una jornada abierta que finalizar.");
        }

        if (endAt <= existing.StartedAt)
        {
            return Error.Validation("La hora de final debe ser posterior al inicio de la jornada.");
        }

        if (existing.CurrentPausedAt is { } pausedAt && endAt < pausedAt)
        {
            return Error.Validation("La hora de final no puede ser anterior a la pausa en curso.");
        }

        existing.End(endAt);

        await _timeEntryRepository.UpdateAsync(existing, cancellationToken);

        return TimeEntryResponse.From(existing);
    }
}
