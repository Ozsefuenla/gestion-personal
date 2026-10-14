using GestionPersonal.Application.Abstractions;
using GestionPersonal.Application.Common;
using GestionPersonal.Domain.Entities;
using GestionPersonal.Domain.Enums;

namespace GestionPersonal.Application.Vacations;

public sealed class RequestVacationCommandHandler
{
    private readonly IWorkerRepository _workerRepository;
    private readonly IVacationRepository _vacationRepository;
    private readonly ICurrentCenter _currentCenter;

    public RequestVacationCommandHandler(
        IWorkerRepository workerRepository,
        IVacationRepository vacationRepository,
        ICurrentCenter currentCenter)
    {
        _workerRepository = workerRepository;
        _vacationRepository = vacationRepository;
        _currentCenter = currentCenter;
    }

    public async Task<Result<VacationResponse>> Handle(
        RequestVacationCommand command,
        CancellationToken cancellationToken = default)
    {
        var workerResult = await WorkerScope.GetScopedWorkerAsync(
            _workerRepository, _currentCenter, command.WorkerId, cancellationToken);

        if (workerResult.IsFailure)
        {
            return workerResult.Error;
        }

        var worker = workerResult.Value;

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

        if (worker.Role == Role.Admin)
        {
            vacation.Approve(now);
        }

        _vacationRepository.Add(vacation);

        return VacationResponse.From(vacation);
    }
}
