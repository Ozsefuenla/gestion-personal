using GestionPersonal.Application.Abstractions;
using GestionPersonal.Application.Common;

namespace GestionPersonal.Application.TimeEntries;

public sealed class EditActiveTimeEntryCommandHandler
{
    private readonly IWorkerRepository _workerRepository;
    private readonly ICurrentUser _currentUser;
    private readonly ITimeEntryRepository _timeEntryRepository;

    public EditActiveTimeEntryCommandHandler(
        IWorkerRepository workerRepository,
        ICurrentUser currentUser,
        ITimeEntryRepository timeEntryRepository)
    {
        _workerRepository = workerRepository;
        _currentUser = currentUser;
        _timeEntryRepository = timeEntryRepository;
    }

    public async Task<Result> Handle(
        int workerId,
        EditActiveTimeEntryCommand command,
        CancellationToken cancellationToken = default)
    {
        var workerResult = await WorkerScope.GetSelfScopedWorkerAsync(
            _workerRepository, _currentUser, workerId, cancellationToken);

        if (workerResult.IsFailure)
        {
            return workerResult.Error;
        }

        var entry = await _timeEntryRepository.GetOpenEntryByWorkerAsync(workerId, cancellationToken);

        if (entry is null)
        {
            return Error.Validation("No hay un fichaje en curso.");
        }

        try
        {
            entry.EditLastActiveStart(command.Start, DateTimeOffset.UtcNow);
        }
        catch (ArgumentException ex)
        {
            return Error.Validation(ex.Message);
        }

        await _timeEntryRepository.UpdateAsync(entry, cancellationToken);

        return Result.Success();
    }
}
