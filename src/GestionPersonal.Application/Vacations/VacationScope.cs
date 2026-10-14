using GestionPersonal.Application.Abstractions;
using GestionPersonal.Application.Common;
using GestionPersonal.Domain.Entities;
using GestionPersonal.Domain.Enums;

namespace GestionPersonal.Application.Vacations;

internal static class VacationScope
{
    public static async Task<Result<Vacation>> GetScopedVacationAsync(
        IWorkerRepository workerRepository,
        IVacationRepository vacationRepository,
        ICurrentCenter currentCenter,
        Guid vacationId,
        CancellationToken cancellationToken = default)
    {
        var centerId = currentCenter.CenterId;

        if (centerId is null)
        {
            return Error.Forbidden("No autenticado.");
        }

        var vacation = await vacationRepository.GetByIdAsync(vacationId, cancellationToken);

        if (vacation is null || vacation.Status == VacationStatus.Deleted)
        {
            return Error.NotFound("La ausencia no existe.");
        }

        var worker = await workerRepository.GetByIdAndCenterAsync(vacation.WorkerId, centerId.Value, cancellationToken);

        if (worker is null)
        {
            return Error.NotFound("La ausencia no existe.");
        }

        return vacation;
    }
}
