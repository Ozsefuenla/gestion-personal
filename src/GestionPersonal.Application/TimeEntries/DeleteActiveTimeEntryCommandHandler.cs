using GestionPersonal.Application.Abstractions;
using GestionPersonal.Application.Common;
using GestionPersonal.Domain.Enums;

namespace GestionPersonal.Application.TimeEntries;

public sealed class DeleteActiveTimeEntryCommandHandler
{
    private readonly ITimeEntryRepository _timeEntryRepository;

    public DeleteActiveTimeEntryCommandHandler(ITimeEntryRepository timeEntryRepository)
    {
        _timeEntryRepository = timeEntryRepository;
    }

    public async Task<Result> Handle(Guid workerId, CancellationToken cancellationToken = default)
    {
        var entry = await _timeEntryRepository.GetOpenEntryByWorkerAsync(workerId, cancellationToken);

        if (entry is null)
        {
            return Error.Validation("No hay un fichaje en curso.");
        }

        if (entry.Status == TimeEntryStatus.InProgress && entry.Pauses.Count == 0)
        {
            _timeEntryRepository.Remove(entry);
        }
        else
        {
            entry.DeleteLastActive();
        }

        return Result.Success();
    }
}
