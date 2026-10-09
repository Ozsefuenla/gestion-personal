using GestionPersonal.Application.Abstractions;
using GestionPersonal.Application.Common;

namespace GestionPersonal.Application.TimeEntries;

public sealed class EditActiveTimeEntryCommandHandler
{
    private readonly ITimeEntryRepository _timeEntryRepository;

    public EditActiveTimeEntryCommandHandler(ITimeEntryRepository timeEntryRepository)
    {
        _timeEntryRepository = timeEntryRepository;
    }

    public async Task<Result> Handle(
        Guid workerId,
        EditActiveTimeEntryCommand command,
        CancellationToken cancellationToken = default)
    {
        var entry = await _timeEntryRepository.GetOpenEntryByWorkerAsync(workerId, cancellationToken);

        if (entry is null)
        {
            return Error.Validation("No hay un fichaje en curso.");
        }

        try
        {
            entry.EditLastActiveStart(command.Start, DateTimeOffset.UtcNow);
        }
        catch (ArgumentException ex)
        {
            return Error.Validation(ex.Message);
        }

        return Result.Success();
    }
}
