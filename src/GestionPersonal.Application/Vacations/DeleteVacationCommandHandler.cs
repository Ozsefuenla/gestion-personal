using GestionPersonal.Application.Abstractions;
using GestionPersonal.Application.Common;
using GestionPersonal.Domain.Enums;

namespace GestionPersonal.Application.Vacations;

public sealed class DeleteVacationCommandHandler
{
    private readonly IWorkerRepository _workerRepository;
    private readonly IVacationRepository _vacationRepository;
    private readonly ICurrentCenter _currentCenter;

    public DeleteVacationCommandHandler(
        IWorkerRepository workerRepository,
        IVacationRepository vacationRepository,
        ICurrentCenter currentCenter)
    {
        _workerRepository = workerRepository;
        _vacationRepository = vacationRepository;
        _currentCenter = currentCenter;
    }

    public async Task<Result> Handle(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var centerId = _currentCenter.CenterId;

        if (centerId is null)
        {
            return Error.Forbidden("No autenticado.");
        }

        var vacation = await _vacationRepository.GetByIdAsync(id, cancellationToken);

        if (vacation is null || vacation.Status == VacationStatus.Deleted)
        {
            return Error.NotFound("La ausencia no existe.");
        }

        var worker = await _workerRepository.GetByIdAndCenterAsync(vacation.WorkerId, centerId.Value, cancellationToken);

        if (worker is null)
        {
            return Error.NotFound("La ausencia no existe.");
        }

        if (vacation.Status == VacationStatus.Approved && worker.Role != Role.Admin)
        {
            return Error.Forbidden("Solo un administrador puede eliminar una ausencia aprobada.");
        }

        vacation.Delete();

        await _vacationRepository.UpdateAsync(vacation, cancellationToken);

        return Result.Success();
    }
}
