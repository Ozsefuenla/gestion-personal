using GestionPersonal.Application.Abstractions;
using GestionPersonal.Application.Common;
using GestionPersonal.Domain.Entities;
using GestionPersonal.Domain.Enums;

namespace GestionPersonal.Application.Workers;

public sealed class CreateWorkerCommandHandler
{
    private readonly IWorkerRepository _workerRepository;
    private readonly ICenterRepository _centerRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ICurrentCenter _currentCenter;

    public CreateWorkerCommandHandler(
        IWorkerRepository workerRepository,
        ICenterRepository centerRepository,
        IPasswordHasher passwordHasher,
        ICurrentCenter currentCenter)
    {
        _workerRepository = workerRepository;
        _centerRepository = centerRepository;
        _passwordHasher = passwordHasher;
        _currentCenter = currentCenter;
    }

    public async Task<Result<WorkerResponse>> Handle(
        CreateWorkerCommand command,
        CancellationToken cancellationToken = default)
    {
        var role = ParseRole(command.Role);
        if (role is null)
        {
            return Error.Validation("El rol debe ser 'admin' o 'worker'.");
        }

        var centerId = _currentCenter.CenterId;

        if (centerId is null)
        {
            return Error.Forbidden("No autenticado.");
        }

        if (command.CenterId != centerId.Value)
        {
            return Error.Forbidden("No puedes crear trabajadores en otro centro.");
        }

        var center = await _centerRepository.GetByIdAsync(command.CenterId, cancellationToken);
        if (center is null)
        {
            return Error.NotFound("El centro no existe.");
        }

        var worker = Worker.Create(command.CenterId, command.FullName, _passwordHasher.Hash(command.Pin), role.Value, mustChangePin: command.MustChangePin);

        var persisted = await _workerRepository.AddAsync(worker, cancellationToken);

        return WorkerResponse.From(persisted);
    }

    private static Role? ParseRole(string role) =>
        role.Trim().ToLowerInvariant() switch
        {
            "admin" => Role.Admin,
            "worker" => Role.Worker,
            _ => null
        };
}
