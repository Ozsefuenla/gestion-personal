using GestionPersonal.Application.Abstractions;
using GestionPersonal.Application.Common;
using GestionPersonal.Domain.Entities;
using GestionPersonal.Domain.Enums;

namespace GestionPersonal.Application.Workers;

public sealed class CreateWorkerCommandHandler
{
    private readonly IWorkerRepository _workerRepository;
    private readonly ICenterRepository _centerRepository;

    public CreateWorkerCommandHandler(
        IWorkerRepository workerRepository,
        ICenterRepository centerRepository)
    {
        _workerRepository = workerRepository;
        _centerRepository = centerRepository;
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

        var center = await _centerRepository.GetByIdAsync(command.CenterId, cancellationToken);
        if (center is null)
        {
            return Error.NotFound("El centro no existe.");
        }

        var workers = await _workerRepository.GetAllAsync(cancellationToken);
        var nextId = workers.Count == 0 ? 1 : workers.Max(w => w.Id) + 1;

        var worker = Worker.Create(nextId, command.CenterId, command.FullName, command.Pin, role.Value);

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
