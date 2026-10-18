using GestionPersonal.Application.Abstractions;
using GestionPersonal.Application.Common;
using GestionPersonal.Domain.Entities;
using GestionPersonal.Domain.Enums;

namespace GestionPersonal.Application.Vacations;

public sealed class RequestVacationCommandHandler
{
    private readonly IWorkerRepository _workerRepository;
    private readonly IVacationRepository _vacationRepository;
    private readonly ICurrentUser _currentUser;

    public RequestVacationCommandHandler(
        IWorkerRepository workerRepository,
        IVacationRepository vacationRepository,
        ICurrentUser currentUser)
    {
        _workerRepository = workerRepository;
        _vacationRepository = vacationRepository;
        _currentUser = currentUser;
    }

    public async Task<Result<VacationResponse>> Handle(
        RequestVacationCommand command,
        CancellationToken cancellationToken = default)
    {
        var workerResult = await WorkerScope.GetSelfScopedWorkerAsync(
            _workerRepository, _currentUser, command.WorkerId, cancellationToken);

        if (workerResult.IsFailure)
        {
            return workerResult.Error;
        }

        var type = VacationTypeHelper.Parse(command.Type);

        if (type is null)
        {
            return Error.Validation("El tipo de ausencia no es válido.");
        }

        if (command.EndDate < command.StartDate)
        {
            return Error.Validation("La fecha de fin no puede ser anterior a la de inicio.");
        }

        var overlaps = await _vacationRepository.HasOverlappingActiveAsync(
            command.WorkerId, command.StartDate, command.EndDate, cancellationToken);

        if (overlaps)
        {
            return Error.Conflict("Ya tienes una ausencia pendiente o aprobada en esas fechas.");
        }

        var now = DateTimeOffset.UtcNow;

        var vacation = Vacation.Request(
            command.WorkerId,
            command.StartDate,
            command.EndDate,
            type.Value,
            now);

        await _vacationRepository.AddAsync(vacation, cancellationToken);

        return VacationResponse.From(vacation);
    }
}
