using GestionPersonal.Application.Abstractions;
using GestionPersonal.Application.Common;
using GestionPersonal.Application.TimeEntries;

namespace GestionPersonal.Application.Workers;

public sealed class LoginCommandHandler
{
    private readonly IWorkerRepository _workerRepository;
    private readonly ITimeEntryRepository _timeEntryRepository;

    public LoginCommandHandler(
        IWorkerRepository workerRepository,
        ITimeEntryRepository timeEntryRepository)
    {
        _workerRepository = workerRepository;
        _timeEntryRepository = timeEntryRepository;
    }

    public async Task<Result<WorkerStatusResponse>> Handle(
        LoginCommand command,
        CancellationToken cancellationToken = default)
    {
        var worker = await _workerRepository.GetByPinAsync(command.Pin, cancellationToken);

        if (worker is null)
        {
            return Error.Validation("El PIN introducido no es correcto.");
        }

        var now = DateTimeOffset.UtcNow;
        var (from, to) = TodayStatusHelper.GetTodayRange(now);

        return await WorkerStatusBuilder.BuildAsync(worker, now, from, to, _timeEntryRepository, cancellationToken);
    }
}
