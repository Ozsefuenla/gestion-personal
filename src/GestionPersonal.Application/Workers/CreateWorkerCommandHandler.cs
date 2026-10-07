using GestionPersonal.Application.Abstractions;
using GestionPersonal.Application.Common;
using GestionPersonal.Domain.Entities;
using GestionPersonal.Domain.Enums;

namespace GestionPersonal.Application.Workers;

public sealed class CreateWorkerCommandHandler
{
    private readonly IWorkerRepository _workerRepository;

    public CreateWorkerCommandHandler(IWorkerRepository workerRepository)
    {
        _workerRepository = workerRepository;
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

        var worker = Worker.Create(command.FullName, command.Pin, role.Value);

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
