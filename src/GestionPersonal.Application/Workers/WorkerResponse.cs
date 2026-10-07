using GestionPersonal.Domain.Entities;

namespace GestionPersonal.Application.Workers;

public sealed record WorkerResponse(Guid Id, string FullName, string Role)
{
    public static WorkerResponse From(Worker worker) =>
        new(worker.Id, worker.FullName, worker.Role.ToString().ToLowerInvariant());
}
