using GestionPersonal.Application.Abstractions;
using GestionPersonal.Application.Common;
using GestionPersonal.Domain.Enums;

namespace GestionPersonal.Application.TimeEntries;

public sealed class ResumeTimeEntryCommandHandler
{
    private readonly IWorkerRepository _workerRepository;
    private readonly ICurrentCenter _currentCenter;
    private readonly ITimeEntryRepository _timeEntryRepository;

    public ResumeTimeEntryCommandHandler(
        IWorkerRepository workerRepository,
        ICurrentCenter currentCenter,
        ITimeEntryRepository timeEntryRepository)
    {
        _workerRepository = workerRepository;
        _currentCenter = currentCenter;
        _timeEntryRepository = timeEntryRepository;
    }

    public async Task<Result<TimeEntryResponse>> Handle(
        ResumeTimeEntryCommand command,
        CancellationToken cancellationToken = default)
    {
        var workerResult = await WorkerScope.GetScopedWorkerAsync(_workerRepository, _currentCenter, command.WorkerId, cancellationToken);

        if (workerResult.IsFailure)
        {
            return workerResult.Error;
        }

        var existing = await _timeEntryRepository.GetOpenEntryByWorkerAsync(command.WorkerId, cancellationToken);

        if (existing is null)
        {
            return Error.Conflict("No hay una jornada en curso que reanudar.");
        }

        if (existing.Status == TimeEntryStatus.InProgress)
        {
            return Error.Conflict("El fichaje no está en pausa.");
        }

        existing.Resume(DateTimeOffset.UtcNow);

        return TimeEntryResponse.From(existing);
    }
}
