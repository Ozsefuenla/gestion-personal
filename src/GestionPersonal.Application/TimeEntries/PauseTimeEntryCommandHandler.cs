using GestionPersonal.Application.Abstractions;
using GestionPersonal.Application.Common;
using GestionPersonal.Domain.Enums;

namespace GestionPersonal.Application.TimeEntries;

public sealed class PauseTimeEntryCommandHandler
{
    private readonly ITimeEntryRepository _timeEntryRepository;

    public PauseTimeEntryCommandHandler(ITimeEntryRepository timeEntryRepository)
    {
        _timeEntryRepository = timeEntryRepository;
    }

    public async Task<Result<TimeEntryResponse>> Handle(
        PauseTimeEntryCommand command,
        CancellationToken cancellationToken = default)
    {
        var existing = await _timeEntryRepository.GetOpenEntryByWorkerAsync(command.WorkerId, cancellationToken);

        if (existing is null)
        {
            return Error.Conflict("No hay una jornada en curso que pausar.");
        }

        if (existing.Status == TimeEntryStatus.Paused)
        {
            return Error.Conflict("La jornada ya está en pausa.");
        }

        existing.Pause(DateTimeOffset.UtcNow);

        return TimeEntryResponse.From(existing);
    }
}
