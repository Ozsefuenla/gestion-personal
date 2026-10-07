using GestionPersonal.Application.Abstractions;
using GestionPersonal.Application.Common;
using GestionPersonal.Domain.Enums;

namespace GestionPersonal.Application.TimeEntries;

public sealed class ResumeTimeEntryCommandHandler
{
    private readonly ITimeEntryRepository _timeEntryRepository;

    public ResumeTimeEntryCommandHandler(ITimeEntryRepository timeEntryRepository)
    {
        _timeEntryRepository = timeEntryRepository;
    }

    public async Task<Result<TimeEntryResponse>> Handle(
        ResumeTimeEntryCommand command,
        CancellationToken cancellationToken = default)
    {
        var existing = await _timeEntryRepository.GetOpenEntryByWorkerAsync(command.WorkerId, cancellationToken);

        if (existing is null)
        {
            return Error.Conflict("No hay una jornada en curso que reanudar.");
        }

        if (existing.Status == TimeEntryStatus.InProgress)
        {
            return Error.Conflict("El fichaje no está en pausa.");
        }

        existing.Resume(DateTimeOffset.UtcNow);

        return TimeEntryResponse.From(existing);
    }
}
