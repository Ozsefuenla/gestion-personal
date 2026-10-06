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
        var email = command.Email.Trim().ToLowerInvariant();

        if (await _workerRepository.EmailExistsAsync(email, cancellationToken))
        {
            return Error.Conflict("Ya existe un trabajador con ese email.");
        }

        var role = ParseRole(command.Role);
        if (role is null)
        {
            return Error.Validation("El rol debe ser 'admin' o 'worker'.");
        }

        var worker = Worker.Create(command.FullName, command.Email, role.Value);

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
