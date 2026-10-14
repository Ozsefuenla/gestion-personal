using GestionPersonal.Application.Abstractions;
using GestionPersonal.Application.Common;

namespace GestionPersonal.Application.Workers;

public sealed class ResetWorkerPinCommandHandler
{
    private readonly IWorkerRepository _workerRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ICurrentCenter _currentCenter;

    public ResetWorkerPinCommandHandler(
        IWorkerRepository workerRepository,
        IPasswordHasher passwordHasher,
        ICurrentCenter currentCenter)
    {
        _workerRepository = workerRepository;
        _passwordHasher = passwordHasher;
        _currentCenter = currentCenter;
    }

    public async Task<Result> Handle(
        int workerId,
        ResetWorkerPinCommand command,
        CancellationToken cancellationToken = default)
    {
        var scoped = await WorkerScope.GetScopedWorkerAsync(
            _workerRepository, _currentCenter, workerId, cancellationToken);

        if (scoped.IsFailure)
        {
            return scoped.Error;
        }

        scoped.Value.ResetPin(_passwordHasher.Hash(command.Pin));

        return Result.Success();
    }
}
