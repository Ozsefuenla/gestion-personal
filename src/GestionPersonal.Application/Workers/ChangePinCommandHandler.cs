using GestionPersonal.Application.Abstractions;
using GestionPersonal.Application.Common;

namespace GestionPersonal.Application.Workers;

public sealed class ChangePinCommandHandler
{
    private readonly IWorkerRepository _workerRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ICurrentCenter _currentCenter;

    public ChangePinCommandHandler(
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
        ChangePinCommand command,
        CancellationToken cancellationToken = default)
    {
        var centerId = _currentCenter.CenterId;

        if (centerId is null)
        {
            return Error.Forbidden("No autenticado.");
        }

        var worker = await _workerRepository.GetByIdAndCenterAsync(workerId, centerId.Value, cancellationToken);

        if (worker is null)
        {
            return Error.NotFound("El trabajador no existe.");
        }

        if (!_passwordHasher.Verify(command.CurrentPin, worker.PinHash))
        {
            return Error.Validation("El PIN actual no es correcto.");
        }

        if (command.NewPin == command.CurrentPin)
        {
            return Error.Validation("El nuevo PIN debe ser distinto.");
        }

        worker.ChangePin(_passwordHasher.Hash(command.NewPin));

        return Result.Success();
    }
}
