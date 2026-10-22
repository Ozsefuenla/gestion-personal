using GestionPersonal.Application.Abstractions;
using GestionPersonal.Application.Common;

namespace GestionPersonal.Application.TimeEntries;

public sealed class CloseStaleTimeEntryCommandHandler
{
    private readonly IWorkerRepository _workerRepository;
    private readonly ICurrentUser _currentUser;
    private readonly EndTimeEntryCommandHandler _endHandler;

    public CloseStaleTimeEntryCommandHandler(
        IWorkerRepository workerRepository,
        ICurrentUser currentUser,
        EndTimeEntryCommandHandler endHandler)
    {
        _workerRepository = workerRepository;
        _currentUser = currentUser;
        _endHandler = endHandler;
    }

    public async Task<Result<TimeEntryResponse>> Handle(
        CloseStaleTimeEntryCommand command,
        CancellationToken cancellationToken = default)
    {
        var workerResult = await WorkerScope.GetSelfScopedWorkerAsync(
            _workerRepository, _currentUser, command.WorkerId, cancellationToken);

        if (workerResult.IsFailure)
        {
            return workerResult.Error;
        }

        var endAt = TimeStamp.Compose(command.Date, command.EndTime);

        return await _endHandler.EndForWorkerAtAsync(command.WorkerId, endAt, cancellationToken);
    }
}
