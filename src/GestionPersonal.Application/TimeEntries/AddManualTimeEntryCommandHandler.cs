using GestionPersonal.Application.Abstractions;
using GestionPersonal.Application.Common;
using GestionPersonal.Domain.Entities;
using GestionPersonal.Domain.Enums;

namespace GestionPersonal.Application.TimeEntries;

public sealed class AddManualTimeEntryCommandHandler
{
    private readonly IWorkerRepository _workerRepository;
    private readonly ICurrentUser _currentUser;
    private readonly ITimeEntryRepository _timeEntryRepository;

    public AddManualTimeEntryCommandHandler(
        IWorkerRepository workerRepository,
        ICurrentUser currentUser,
        ITimeEntryRepository timeEntryRepository)
    {
        _workerRepository = workerRepository;
        _currentUser = currentUser;
        _timeEntryRepository = timeEntryRepository;
    }

    public async Task<Result<TimeEntryResponse>> Handle(
        AddManualTimeEntryCommand command,
        CancellationToken cancellationToken = default)
    {
        var workerResult = await WorkerScope.GetSelfScopedWorkerAsync(_workerRepository, _currentUser, command.WorkerId, cancellationToken);

        if (workerResult.IsFailure)
        {
            return workerResult.Error;
        }

        var type = command.Type!.Trim().ToLowerInvariant();
        var (dayFrom, dayTo) = GetDayRange(command.Date!.Value);
        var openEntry = await _timeEntryRepository.GetOpenEntryByWorkerBetweenAsync(
            command.WorkerId, dayFrom, dayTo, cancellationToken);

        switch (type)
        {
            case "start":
                if (openEntry is not null)
                {
                    return Error.Conflict("La jornada de ese día ya está iniciada.");
                }

                var startTimestamp = TimeStamp.Compose(command.Date!.Value, command.StartTime!.Value);

                if (await OverlapsExistingAsync(command.WorkerId, startTimestamp, cancellationToken))
                {
                    return Error.Conflict("Ya existe un marcaje en ese horario.");
                }

                var started = TimeEntry.Start(command.WorkerId, startTimestamp);
                await _timeEntryRepository.AddAsync(started, cancellationToken);

                return TimeEntryResponse.From(started);

            case "pause":
                if (openEntry is null)
                {
                    return Error.Conflict("No hay una jornada en curso que pausar.");
                }

                if (openEntry.Status == TimeEntryStatus.Paused)
                {
                    return Error.Conflict("La jornada ya está en pausa.");
                }

                var pauseTimestamp = TimeStamp.Compose(command.Date!.Value, command.StartTime!.Value);

                if (pauseTimestamp <= openEntry.StartedAt)
                {
                    return Error.Validation("La hora de pausa debe ser posterior al inicio de la jornada.");
                }

                openEntry.Pause(pauseTimestamp);

                if (command.EndTime is not null)
                {
                    openEntry.Resume(TimeStamp.Compose(command.Date.Value, command.EndTime.Value));
                }

                await _timeEntryRepository.UpdateAsync(openEntry, cancellationToken);

                return TimeEntryResponse.From(openEntry);

            case "end":
                if (openEntry is null)
                {
                    return Error.Conflict("No hay una jornada abierta que finalizar.");
                }

                var endTimestamp = TimeStamp.Compose(command.Date!.Value, command.EndTime!.Value);

                if (endTimestamp <= openEntry.StartedAt)
                {
                    return Error.Validation("La hora de final debe ser posterior al inicio de la jornada.");
                }

                openEntry.End(endTimestamp);

                await _timeEntryRepository.UpdateAsync(openEntry, cancellationToken);

                return TimeEntryResponse.From(openEntry);

            default:
                return Error.Validation("Tipo de marcaje no válido.");
        }
    }

    private async Task<bool> OverlapsExistingAsync(int workerId, DateTimeOffset timestamp, CancellationToken cancellationToken)
    {
        var overlapping = await _timeEntryRepository.GetByWorkerOverlappingAsync(
            workerId,
            timestamp,
            timestamp.AddTicks(1),
            cancellationToken);

        return overlapping.Count > 0;
    }

    private static (DateTimeOffset From, DateTimeOffset To) GetDayRange(DateOnly date)
    {
        var from = TimeStamp.Compose(date, TimeOnly.MinValue);
        return (from, from.AddDays(1));
    }

}
