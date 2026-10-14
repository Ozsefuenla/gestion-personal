using GestionPersonal.Application.Abstractions;
using GestionPersonal.Application.Common;

namespace GestionPersonal.Application.Workers;

public sealed class RenameWorkerCommandHandler
{
    private readonly IWorkerRepository _workerRepository;
    private readonly ICurrentCenter _currentCenter;

    public RenameWorkerCommandHandler(
        IWorkerRepository workerRepository,
        ICurrentCenter currentCenter)
    {
        _workerRepository = workerRepository;
        _currentCenter = currentCenter;
    }

    public async Task<Result> Handle(
        int workerId,
        RenameWorkerCommand command,
        CancellationToken cancellationToken = default)
    {
        var scoped = await WorkerScope.GetScopedWorkerAsync(
            _workerRepository, _currentCenter, workerId, cancellationToken);

        if (scoped.IsFailure)
        {
            return scoped.Error;
        }

        scoped.Value.Rename(command.FullName);

        return Result.Success();
    }
}
