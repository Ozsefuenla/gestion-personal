using GestionPersonal.Application.Abstractions;
using GestionPersonal.Application.Common;
using GestionPersonal.Application.TimeEntries;

namespace GestionPersonal.Application.Workers;

public sealed class GetWorkerStatusQueryHandler
{
    private readonly IWorkerRepository _workerRepository;
    private readonly ITimeEntryRepository _timeEntryRepository;
    private readonly ICurrentCenter _currentCenter;

    public GetWorkerStatusQueryHandler(
        IWorkerRepository workerRepository,
        ITimeEntryRepository timeEntryRepository,
        ICurrentCenter currentCenter)
    {
        _workerRepository = workerRepository;
        _timeEntryRepository = timeEntryRepository;
        _currentCenter = currentCenter;
    }

    public async Task<Result<WorkerStatusResponse>> Handle(
        int workerId,
        CancellationToken cancellationToken = default)
    {
        var centerId = _currentCenter.CenterId;

        if (centerId is null)
        {
            return Error.Forbidden("No autenticado.");
        }

        var worker = await _workerRepository.GetByIdAndCenterAsync(workerId, centerId.Value, cancellationToken);

        if (worker is null)
        {
            return Error.NotFound("El trabajador no existe.");
        }

        var now = DateTimeOffset.UtcNow;
        var (from, to) = TodayStatusHelper.GetTodayRange(now);

        return await WorkerStatusBuilder.BuildAsync(worker, now, from, to, _timeEntryRepository, cancellationToken);
    }
}
