using GestionPersonal.Domain.Entities;

namespace GestionPersonal.Application.Workers;

public sealed record WorkerResponse(int Id, string FullName, bool Active, bool MustChangePin)
{
    public static WorkerResponse From(Worker worker) =>
        new(worker.Id, worker.FullName, worker.Active, worker.MustChangePin);
}
