using GestionPersonal.Domain.Entities;

namespace GestionPersonal.Application.Workers;

public sealed record WorkerResponse(int Id, string FullName, string Role)
{
    public static WorkerResponse From(Worker worker) =>
        new(worker.Id, worker.FullName, worker.Role.ToString().ToLowerInvariant());
}
