using GestionPersonal.Application.Abstractions;
using GestionPersonal.Application.Common;
using GestionPersonal.Application.TimeEntries;

namespace GestionPersonal.Application.Workers;

public sealed class GetWorkerStatusQueryHandler
{
    private readonly IWorkerRepository _workerRepository;
    private readonly ITimeEntryRepository _timeEntryRepository;

    public GetWorkerStatusQueryHandler(
        IWorkerRepository workerRepository,
        ITimeEntryRepository timeEntryRepository)
    {
        _workerRepository = workerRepository;
        _timeEntryRepository = timeEntryRepository;
    }

    public async Task<Result<WorkerStatusResponse>> Handle(
        Guid workerId,
        CancellationToken cancellationToken = default)
    {
        var worker = await _workerRepository.GetByIdAsync(workerId, cancellationToken);

        if (worker is null)
        {
            return Error.NotFound("El trabajador no existe.");
        }

        var now = DateTimeOffset.UtcNow;
        var (from, to) = TodayStatusHelper.GetTodayRange(now);

        return await WorkerStatusBuilder.BuildAsync(worker, now, from, to, _timeEntryRepository, cancellationToken);
    }
}
