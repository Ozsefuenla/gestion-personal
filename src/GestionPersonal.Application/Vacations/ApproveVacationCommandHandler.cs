using GestionPersonal.Application.Abstractions;
using GestionPersonal.Application.Common;
using GestionPersonal.Domain.Enums;

namespace GestionPersonal.Application.Vacations;

public sealed class ApproveVacationCommandHandler
{
    private readonly IWorkerRepository _workerRepository;
    private readonly IVacationRepository _vacationRepository;
    private readonly ICurrentUser _currentUser;

    public ApproveVacationCommandHandler(
        IWorkerRepository workerRepository,
        IVacationRepository vacationRepository,
        ICurrentUser currentUser)
    {
        _workerRepository = workerRepository;
        _vacationRepository = vacationRepository;
        _currentUser = currentUser;
    }

    public async Task<Result<VacationResponse>> Handle(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var scoped = await VacationScope.GetAdminScopedVacationAsync(
            _workerRepository, _vacationRepository, _currentUser, id, cancellationToken);

        if (scoped.IsFailure)
        {
            return scoped.Error;
        }

        var vacation = scoped.Value;

        if (vacation.Status != VacationStatus.Pending)
        {
            return Error.Conflict("Solo se puede aprobar una ausencia pendiente.");
        }

        vacation.Approve(DateTimeOffset.UtcNow);

        await _vacationRepository.UpdateAsync(vacation, cancellationToken);

        return VacationResponse.From(vacation);
    }
}
