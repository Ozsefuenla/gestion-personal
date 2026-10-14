using GestionPersonal.Application.Abstractions;
using GestionPersonal.Application.Common;
using GestionPersonal.Domain.Enums;

namespace GestionPersonal.Application.Vacations;

public sealed class RejectVacationCommandHandler
{
    private readonly IWorkerRepository _workerRepository;
    private readonly IVacationRepository _vacationRepository;
    private readonly ICurrentCenter _currentCenter;

    public RejectVacationCommandHandler(
        IWorkerRepository workerRepository,
        IVacationRepository vacationRepository,
        ICurrentCenter currentCenter)
    {
        _workerRepository = workerRepository;
        _vacationRepository = vacationRepository;
        _currentCenter = currentCenter;
    }

    public async Task<Result<VacationResponse>> Handle(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var scoped = await VacationScope.GetScopedVacationAsync(
            _workerRepository, _vacationRepository, _currentCenter, id, cancellationToken);

        if (scoped.IsFailure)
        {
            return scoped.Error;
        }

        var vacation = scoped.Value;

        if (vacation.Status != VacationStatus.Pending)
        {
            return Error.Conflict("Solo se puede denegar una ausencia pendiente.");
        }

        vacation.Reject(DateTimeOffset.UtcNow);

        return VacationResponse.From(vacation);
    }
}
