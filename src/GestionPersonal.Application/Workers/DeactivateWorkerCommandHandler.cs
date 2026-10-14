using GestionPersonal.Application.Abstractions;
using GestionPersonal.Application.Common;
using GestionPersonal.Domain.Enums;

namespace GestionPersonal.Application.Workers;

public sealed class DeactivateWorkerCommandHandler
{
    private readonly IWorkerRepository _workerRepository;
    private readonly ICurrentCenter _currentCenter;

    public DeactivateWorkerCommandHandler(
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

        if (scoped.Value.Role == Role.Admin)
        {
            return Error.Validation("No se puede dar de baja a un administrador.");
        }

        scoped.Value.Deactivate();

        return Result.Success();
    }
}
