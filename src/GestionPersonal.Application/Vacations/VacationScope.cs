using GestionPersonal.Application.Abstractions;
using GestionPersonal.Application.Common;
using GestionPersonal.Domain.Entities;
using GestionPersonal.Domain.Enums;

namespace GestionPersonal.Application.Vacations;

internal static class VacationScope
{
    public static async Task<Result<Vacation>> GetAdminScopedVacationAsync(
        IWorkerRepository workerRepository,
        IVacationRepository vacationRepository,
        ICurrentUser currentUser,
        Guid vacationId,
        CancellationToken cancellationToken = default)
    {
        if (AccessControl.RequireCenterAdmin(currentUser) is { } error)
        {
            return error;
        }

        return await GetInCenterAsync(
            workerRepository, vacationRepository, currentUser.CenterId!.Value, vacationId, cancellationToken);
    }

    public static async Task<Result<Vacation>> GetSelfOrAdminScopedVacationAsync(
        IWorkerRepository workerRepository,
        IVacationRepository vacationRepository,
        ICurrentUser currentUser,
        Guid vacationId,
        CancellationToken cancellationToken = default)
    {
        if (AccessControl.RequireCenterMember(currentUser) is { } error)
        {
            return error;
        }

        var vacation = await vacationRepository.GetByIdAsync(vacationId, cancellationToken);

        if (vacation is null || vacation.Status == VacationStatus.Deleted)
        {
            return Error.NotFound("La ausencia no existe.");
        }

        if (AccessControl.RequireSelfOrAdmin(currentUser, vacation.WorkerId) is { } selfError)
        {
            return selfError;
        }

        return await GetInCenterAsync(
            workerRepository, vacationRepository, currentUser.CenterId!.Value, vacationId, cancellationToken);
    }

    private static async Task<Result<Vacation>> GetInCenterAsync(
        IWorkerRepository workerRepository,
        IVacationRepository vacationRepository,
        int centerId,
        Guid vacationId,
        CancellationToken cancellationToken)
    {
        var vacation = await vacationRepository.GetByIdAsync(vacationId, cancellationToken);

        if (vacation is null || vacation.Status == VacationStatus.Deleted)
        {
            return Error.NotFound("La ausencia no existe.");
        }

        var worker = await workerRepository.GetByIdAndCenterAsync(vacation.WorkerId, centerId, cancellationToken);

        if (worker is null)
        {
            return Error.NotFound("La ausencia no existe.");
        }

        return vacation;
    }
}
