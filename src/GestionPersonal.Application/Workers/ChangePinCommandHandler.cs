using GestionPersonal.Application.Abstractions;
using GestionPersonal.Application.Common;

namespace GestionPersonal.Application.Workers;

public sealed class ChangePinCommandHandler
{
    private readonly IWorkerRepository _workerRepository;

    public ChangePinCommandHandler(IWorkerRepository workerRepository)
    {
        _workerRepository = workerRepository;
    }

    public async Task<Result> Handle(
        int workerId,
        ChangePinCommand command,
        CancellationToken cancellationToken = default)
    {
        var worker = await _workerRepository.GetByIdAsync(workerId, cancellationToken);

        if (worker is null)
        {
            return Error.NotFound("El trabajador no existe.");
        }

        if (worker.PinHash != command.CurrentPin)
        {
            return Error.Validation("El PIN actual no es correcto.");
        }

        if (command.NewPin == command.CurrentPin)
        {
            return Error.Validation("El nuevo PIN debe ser distinto.");
        }

        worker.ChangePin(command.NewPin);

        return Result.Success();
    }
}
