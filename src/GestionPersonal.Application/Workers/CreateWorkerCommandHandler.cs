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

        var workers = await _workerRepository.GetAllAsync(cancellationToken);
        var nextId = workers.Count == 0 ? 1 : workers.Max(w => w.Id) + 1;

        var worker = Worker.Create(nextId, command.CenterId, command.FullName, _passwordHasher.Hash(command.Pin), role.Value);

        _workerRepository.Add(worker);

        return WorkerResponse.From(worker);
    }

    private static Role? ParseRole(string role) =>
        role.Trim().ToLowerInvariant() switch
        {
            "admin" => Role.Admin,
            "worker" => Role.Worker,
            _ => null
        };
}
