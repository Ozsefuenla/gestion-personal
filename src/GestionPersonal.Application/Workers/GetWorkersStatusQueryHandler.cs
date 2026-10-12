using GestionPersonal.Application.Abstractions;
using GestionPersonal.Application.Common;
using GestionPersonal.Application.TimeEntries;

namespace GestionPersonal.Application.Workers;

public sealed class GetWorkersStatusQueryHandler
{
    private readonly IWorkerRepository _workerRepository;
    private readonly ITimeEntryRepository _timeEntryRepository;
    private readonly ICurrentCenter _currentCenter;

    public GetWorkersStatusQueryHandler(
        IWorkerRepository workerRepository,
        ITimeEntryRepository timeEntryRepository,
        ICurrentCenter currentCenter)
    {
        _workerRepository = workerRepository;
        _timeEntryRepository = timeEntryRepository;
        _currentCenter = currentCenter;
    }

    public async Task<Result<IReadOnlyList<WorkerStatusResponse>>> Handle(
        CancellationToken cancellationToken = default)
    {
        var centerId = _currentCenter.CenterId;

        if (centerId is null)
        {
            return Error.Forbidden("No autenticado.");
        }

        var now = DateTimeOffset.UtcNow;
        var (from, to) = TodayStatusHelper.GetTodayRange(now);

        var workers = await _workerRepository.GetByCenterAsync(centerId.Value, cancellationToken);

        var result = new List<WorkerStatusResponse>();

        foreach (var worker in workers)
        {
            result.Add(await WorkerStatusBuilder.BuildAsync(worker, now, from, to, _timeEntryRepository, cancellationToken));
        }

        return Result<IReadOnlyList<WorkerStatusResponse>>.Success(result);
    }
}
