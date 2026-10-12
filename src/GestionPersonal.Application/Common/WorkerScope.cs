using GestionPersonal.Application.Abstractions;
using GestionPersonal.Domain.Entities;

namespace GestionPersonal.Application.Common;

public static class WorkerScope
{
    public static async Task<Result<Worker>> GetScopedWorkerAsync(
        IWorkerRepository workerRepository,
        ICurrentCenter currentCenter,
        int workerId,
        CancellationToken cancellationToken = default)
    {
        var centerId = currentCenter.CenterId;

        if (centerId is null)
        {
            return Error.Forbidden("No autenticado.");
        }

        var worker = await workerRepository.GetByIdAndCenterAsync(workerId, centerId.Value, cancellationToken);

        if (worker is null)
        {
            return Error.NotFound("El trabajador no existe.");
        }

        return worker;
    }
}
