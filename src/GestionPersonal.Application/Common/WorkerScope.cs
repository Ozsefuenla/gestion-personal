using GestionPersonal.Application.Abstractions;
using GestionPersonal.Domain.Entities;

namespace GestionPersonal.Application.Common;

public static class WorkerScope
{
    public static async Task<Result<Worker>> GetAdminScopedWorkerAsync(
        IWorkerRepository workerRepository,
        ICurrentUser currentUser,
        int workerId,
        CancellationToken cancellationToken = default)
    {
        if (AccessControl.RequireCenterAdmin(currentUser) is { } error)
        {
            return error;
        }

        return await GetInCenterAsync(workerRepository, currentUser.CenterId!.Value, workerId, cancellationToken);
    }

    public static async Task<Result<Worker>> GetSelfScopedWorkerAsync(
        IWorkerRepository workerRepository,
        ICurrentUser currentUser,
        int workerId,
        CancellationToken cancellationToken = default)
    {
        if (AccessControl.RequireSelfOrAdmin(currentUser, workerId) is { } error)
        {
            return error;
        }

        return await GetInCenterAsync(workerRepository, currentUser.CenterId!.Value, workerId, cancellationToken);
    }

    public static async Task<Result<Worker>> GetChangePinScopedWorkerAsync(
        IWorkerRepository workerRepository,
        ICurrentUser currentUser,
        int workerId,
        CancellationToken cancellationToken = default)
    {
        var selfOrAdmin = AccessControl.RequireSelfOrAdmin(currentUser, workerId) is null;
        var kiosk = AccessControl.RequireKiosk(currentUser) is null;

        if (!selfOrAdmin && !kiosk)
        {
            return Error.Forbidden("No autorizado.");
        }

        return await GetInCenterAsync(workerRepository, currentUser.CenterId!.Value, workerId, cancellationToken);
    }

    private static async Task<Result<Worker>> GetInCenterAsync(
        IWorkerRepository workerRepository,
        int centerId,
        int workerId,
        CancellationToken cancellationToken)
    {
        var worker = await workerRepository.GetByIdAndCenterAsync(workerId, centerId, cancellationToken);

        if (worker is null)
        {
            return Error.NotFound("El trabajador no existe.");
        }

        return worker;
    }
}
