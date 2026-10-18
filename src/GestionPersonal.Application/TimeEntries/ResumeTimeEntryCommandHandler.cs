using GestionPersonal.Application.Abstractions;
using GestionPersonal.Application.Common;
using GestionPersonal.Domain.Enums;

namespace GestionPersonal.Application.TimeEntries;

public sealed class ResumeTimeEntryCommandHandler
{
    private readonly IWorkerRepository _workerRepository;
    private readonly ICurrentUser _currentUser;
    private readonly ITimeEntryRepository _timeEntryRepository;

    public ResumeTimeEntryCommandHandler(
        IWorkerRepository workerRepository,
        ICurrentUser currentUser,
        ITimeEntryRepository timeEntryRepository)
    {
        _workerRepository = workerRepository;
        _currentUser = currentUser;
        _timeEntryRepository = timeEntryRepository;
    }

    public async Task<Result<TimeEntryResponse>> Handle(
        ResumeTimeEntryCommand command,
        CancellationToken cancellationToken = default)
    {
        var workerResult = await WorkerScope.GetSelfScopedWorkerAsync(
            _workerRepository, _currentUser, command.WorkerId, cancellationToken);

        if (workerResult.IsFailure)
        {
            return workerResult.Error;
        }

        return await ResumeForWorkerAsync(command.WorkerId, cancellationToken);
    }

    internal async Task<Result<TimeEntryResponse>> ResumeForWorkerAsync(
        int workerId,
        CancellationToken cancellationToken = default)
    {
        var existing = await _timeEntryRepository.GetOpenEntryByWorkerAsync(workerId, cancellationToken);

        if (existing is null)
        {
            return Error.Conflict("No hay una jornada en curso que reanudar.");
        }

        if (existing.Status == TimeEntryStatus.InProgress)
        {
            return Error.Conflict("El fichaje no está en pausa.");
        }

        existing.Resume(DateTimeOffset.UtcNow);

        await _timeEntryRepository.UpdateAsync(existing, cancellationToken);

        return TimeEntryResponse.From(existing);
    }
}
