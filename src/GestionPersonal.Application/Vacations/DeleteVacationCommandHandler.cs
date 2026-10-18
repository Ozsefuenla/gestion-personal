using GestionPersonal.Application.Abstractions;
using GestionPersonal.Application.Common;
using GestionPersonal.Domain.Enums;

namespace GestionPersonal.Application.Vacations;

public sealed class DeleteVacationCommandHandler
{
    private readonly IWorkerRepository _workerRepository;
    private readonly IVacationRepository _vacationRepository;
    private readonly ICurrentUser _currentUser;

    public DeleteVacationCommandHandler(
        IWorkerRepository workerRepository,
        IVacationRepository vacationRepository,
        ICurrentUser currentUser)
    {
        _workerRepository = workerRepository;
        _vacationRepository = vacationRepository;
        _currentUser = currentUser;
    }

    public async Task<Result> Handle(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var scoped = await VacationScope.GetSelfOrAdminScopedVacationAsync(
            _workerRepository, _vacationRepository, _currentUser, id, cancellationToken);

        if (scoped.IsFailure)
        {
            return scoped.Error;
        }

        var vacation = scoped.Value;

        if (vacation.Status == VacationStatus.Approved && _currentUser.Role != UserRole.CenterAdmin)
        {
            return Error.Forbidden("Solo un administrador puede eliminar una ausencia aprobada.");
        }

        vacation.Delete();

        await _vacationRepository.UpdateAsync(vacation, cancellationToken);

        return Result.Success();
    }
}
