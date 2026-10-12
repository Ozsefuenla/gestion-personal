using GestionPersonal.Application.Abstractions;
using GestionPersonal.Application.Common;

namespace GestionPersonal.Application.TimeEntries;

public sealed class EndTimeEntryCommandHandler
{
    private readonly IWorkerRepository _workerRepository;
    private readonly ICurrentCenter _currentCenter;
    private readonly ITimeEntryRepository _timeEntryRepository;

    public EndTimeEntryCommandHandler(
        IWorkerRepository workerRepository,
        ICurrentCenter currentCenter,
        ITimeEntryRepository timeEntryRepository)
    {
        _workerRepository = workerRepository;
        _currentCenter = currentCenter;
        _timeEntryRepository = timeEntryRepository;
    }

    public async Task<Result<TimeEntryResponse>> Handle(
        EndTimeEntryCommand command,
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
            return Error.Conflict("No hay una jornada abierta que finalizar.");
        }

        existing.End(DateTimeOffset.UtcNow);

        return TimeEntryResponse.From(existing);
    }
}
