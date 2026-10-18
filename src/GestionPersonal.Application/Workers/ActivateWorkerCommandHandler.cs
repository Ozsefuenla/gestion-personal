using GestionPersonal.Application.Abstractions;
using GestionPersonal.Application.Common;

namespace GestionPersonal.Application.Workers;

public sealed class ActivateWorkerCommandHandler
{
    private readonly IWorkerRepository _workerRepository;
    private readonly ICurrentCenter _currentCenter;

    public ActivateWorkerCommandHandler(
        IWorkerRepository workerRepository,
        ICurrentCenter currentCenter)
    {
        _workerRepository = workerRepository;
        _currentCenter = currentCenter;
    }

    public async Task<Result> Handle(int workerId, CancellationToken cancellationToken = default)
    {
        var scoped = await WorkerScope.GetScopedWorkerAsync(
            _workerRepository, _currentCenter, workerId, cancellationToken);

        if (scoped.IsFailure)
        {
            return scoped.Error;
        }

        scoped.Value.Activate();

        await _workerRepository.UpdateAsync(scoped.Value, cancellationToken);

        return Result.Success();
    }
}
