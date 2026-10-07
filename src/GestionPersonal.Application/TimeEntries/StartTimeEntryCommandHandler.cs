using GestionPersonal.Application.Abstractions;
using GestionPersonal.Application.Common;
using GestionPersonal.Domain.Entities;

namespace GestionPersonal.Application.TimeEntries;

public sealed class StartTimeEntryCommandHandler
{
    private readonly ITimeEntryRepository _timeEntryRepository;

    public StartTimeEntryCommandHandler(ITimeEntryRepository timeEntryRepository)
    {
        _timeEntryRepository = timeEntryRepository;
    }

    public async Task<Result<TimeEntryResponse>> Handle(
        StartTimeEntryCommand command,
        CancellationToken cancellationToken = default)
    {
        var existing = await _timeEntryRepository.GetOpenEntryByWorkerAsync(command.WorkerId, cancellationToken);

        if (existing is not null)
        {
            return Error.Conflict("La jornada ya está iniciada.");
        }

        var timeEntry = TimeEntry.Start(command.WorkerId, DateTimeOffset.UtcNow);

        _timeEntryRepository.Add(timeEntry);

        return TimeEntryResponse.From(timeEntry);
    }
}
