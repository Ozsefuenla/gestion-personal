using GestionPersonal.Application.Abstractions;
using GestionPersonal.Application.Common;
using GestionPersonal.Application.TimeEntries;

namespace GestionPersonal.Application.Workers;

public sealed class GetWorkersStatusQueryHandler
{
    private readonly IWorkerRepository _workerRepository;
    private readonly ITimeEntryRepository _timeEntryRepository;

    public GetWorkersStatusQueryHandler(
        IWorkerRepository workerRepository,
        ITimeEntryRepository timeEntryRepository)
    {
        _workerRepository = workerRepository;
        _timeEntryRepository = timeEntryRepository;
    }

    public async Task<Result<IReadOnlyList<WorkerStatusResponse>>> Handle(
        CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        var (from, to) = TodayStatusHelper.GetTodayRange(now);

        var workers = await _workerRepository.GetAllAsync(cancellationToken);

        var result = new List<WorkerStatusResponse>();

        foreach (var worker in workers)
        {
            result.Add(await WorkerStatusBuilder.BuildAsync(worker, now, from, to, _timeEntryRepository, cancellationToken));
        }

        return Result<IReadOnlyList<WorkerStatusResponse>>.Success(result);
    }
}
