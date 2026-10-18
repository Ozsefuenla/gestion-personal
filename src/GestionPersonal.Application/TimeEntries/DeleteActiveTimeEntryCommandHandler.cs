using GestionPersonal.Application.Abstractions;
using GestionPersonal.Application.Common;
using GestionPersonal.Domain.Enums;

namespace GestionPersonal.Application.TimeEntries;

public sealed class DeleteActiveTimeEntryCommandHandler
{
    private readonly IWorkerRepository _workerRepository;
    private readonly ICurrentCenter _currentCenter;
    private readonly ITimeEntryRepository _timeEntryRepository;

    public DeleteActiveTimeEntryCommandHandler(
        IWorkerRepository workerRepository,
        ICurrentCenter currentCenter,
        ITimeEntryRepository timeEntryRepository)
    {
        _workerRepository = workerRepository;
        _currentCenter = currentCenter;
        _timeEntryRepository = timeEntryRepository;
    }

    public async Task<Result> Handle(int workerId, CancellationToken cancellationToken = default)
    {
        var workerResult = await WorkerScope.GetScopedWorkerAsync(_workerRepository, _currentCenter, workerId, cancellationToken);

        if (workerResult.IsFailure)
        {
            return workerResult.Error;
        }

        var entry = await _timeEntryRepository.GetOpenEntryByWorkerAsync(workerId, cancellationToken);

        if (entry is null)
        {
            return Error.Validation("No hay un fichaje en curso.");
        }

        if (entry.Status == TimeEntryStatus.InProgress && entry.Pauses.Count == 0)
        {
            await _timeEntryRepository.RemoveAsync(entry, cancellationToken);
        }
        else
        {
            entry.DeleteLastActive();
            await _timeEntryRepository.UpdateAsync(entry, cancellationToken);
        }

        return Result.Success();
    }
}
