using GestionPersonal.Domain.Entities;

namespace GestionPersonal.Application.Workers;

public sealed record WorkerResponse(Guid Id, string FullName, string Email, string Role)
{
    public static WorkerResponse From(Worker worker) =>
        new(worker.Id, worker.FullName, worker.Email, worker.Role.ToString().ToLowerInvariant());
}
