using GestionPersonal.Application.Abstractions;
using GestionPersonal.Application.Common;

namespace GestionPersonal.Application.Workers;

public sealed class RenameWorkerCommandHandler
{
    private readonly IWorkerRepository _workerRepository;
    private readonly ICurrentUser _currentUser;

    public RenameWorkerCommandHandler(
        IWorkerRepository workerRepository,
        ICurrentUser currentUser)
    {
        _workerRepository = workerRepository;
        _currentUser = currentUser;
    }

    public async Task<Result> Handle(
        int workerId,
        RenameWorkerCommand command,
        CancellationToken cancellationToken = default)
    {
        var scoped = await WorkerScope.GetAdminScopedWorkerAsync(
            _workerRepository, _currentUser, workerId, cancellationToken);

        if (scoped.IsFailure)
        {
            return scoped.Error;
        }

        scoped.Value.Rename(command.FullName);

        await _workerRepository.UpdateAsync(scoped.Value, cancellationToken);

        return Result.Success();
    }
}
