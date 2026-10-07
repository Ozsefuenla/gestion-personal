using GestionPersonal.Application.Abstractions;
using GestionPersonal.Application.Common;

namespace GestionPersonal.Application.TimeEntries;

public sealed class EndTimeEntryCommandHandler
{
    private readonly ITimeEntryRepository _timeEntryRepository;

    public EndTimeEntryCommandHandler(ITimeEntryRepository timeEntryRepository)
    {
        _timeEntryRepository = timeEntryRepository;
    }

    public async Task<Result<TimeEntryResponse>> Handle(
        EndTimeEntryCommand command,
        CancellationToken cancellationToken = default)
    {
        var existing = await _timeEntryRepository.GetOpenEntryByWorkerAsync(command.WorkerId, cancellationToken);

        if (existing is null)
        {
            return Error.Conflict("No hay una jornada abierta que finalizar.");
        }

        existing.End(DateTimeOffset.UtcNow);

        return TimeEntryResponse.From(existing);
    }
}
