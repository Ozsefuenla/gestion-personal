using GestionPersonal.Application.Abstractions;
using GestionPersonal.Application.Common;

namespace GestionPersonal.Application.Workers;

public sealed class ResetWorkerPinCommandHandler
{
    private readonly IWorkerRepository _workerRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ICurrentUser _currentUser;

    public ResetWorkerPinCommandHandler(
        IWorkerRepository workerRepository,
        IPasswordHasher passwordHasher,
        ICurrentUser currentUser)
    {
        _workerRepository = workerRepository;
        _passwordHasher = passwordHasher;
        _currentUser = currentUser;
    }

    public async Task<Result> Handle(
        int workerId,
        ResetWorkerPinCommand command,
        CancellationToken cancellationToken = default)
    {
        var scoped = await WorkerScope.GetAdminScopedWorkerAsync(
            _workerRepository, _currentUser, workerId, cancellationToken);

        if (scoped.IsFailure)
        {
            return scoped.Error;
        }

        scoped.Value.ResetPin(_passwordHasher.Hash(command.Pin));

        await _workerRepository.UpdateAsync(scoped.Value, cancellationToken);

        return Result.Success();
    }
}
