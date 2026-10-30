using GestionPersonal.Application.Abstractions;
using GestionPersonal.Application.Common;

namespace GestionPersonal.Application.Workers;

public sealed class ChangePinCommandHandler
{
    private readonly IWorkerRepository _workerRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ICurrentUser _currentUser;

    public ChangePinCommandHandler(
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
        ChangePinCommand command,
        CancellationToken cancellationToken = default)
    {
        var scoped = await WorkerScope.GetChangePinScopedWorkerAsync(
            _workerRepository, _currentUser, workerId, cancellationToken);

        if (scoped.IsFailure)
        {
            return scoped.Error;
        }

        var worker = scoped.Value;

        if (!_passwordHasher.Verify(command.CurrentPin, worker.PinHash))
        {
            return Error.Validation("El PIN actual no es correcto.");
        }

        if (command.NewPin == command.CurrentPin)
        {
            return Error.Validation("El nuevo PIN debe ser distinto.");
        }

        worker.ChangePin(_passwordHasher.Hash(command.NewPin));

        await _workerRepository.UpdateAsync(worker, cancellationToken);

        return Result.Success();
    }
}
