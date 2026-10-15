using GestionPersonal.Application.Abstractions;
using GestionPersonal.Application.Common;
using GestionPersonal.Domain.Entities;
using GestionPersonal.Domain.Enums;

namespace GestionPersonal.Application.TimeEntries;

public sealed class AddManualTimeEntryCommandHandler
{
    private readonly IWorkerRepository _workerRepository;
    private readonly ICurrentCenter _currentCenter;
    private readonly ITimeEntryRepository _timeEntryRepository;

    public AddManualTimeEntryCommandHandler(
        IWorkerRepository workerRepository,
        ICurrentCenter currentCenter,
        ITimeEntryRepository timeEntryRepository)
    {
        _workerRepository = workerRepository;
        _currentCenter = currentCenter;
        _timeEntryRepository = timeEntryRepository;
    }

    public async Task<Result<TimeEntryResponse>> Handle(
        AddManualTimeEntryCommand command,
        CancellationToken cancellationToken = default)
    {
        var workerResult = await WorkerScope.GetScopedWorkerAsync(_workerRepository, _currentCenter, command.WorkerId, cancellationToken);

        if (workerResult.IsFailure)
        {
            return workerResult.Error;
        }

        var type = command.Type!.Trim().ToLowerInvariant();
        var openEntry = await _timeEntryRepository.GetOpenEntryByWorkerAsync(command.WorkerId, cancellationToken);

        switch (type)
        {
            case "start":
                if (openEntry is not null)
                {
                    return Error.Conflict("La jornada ya está iniciada.");
                }

                var startTimestamp = Combine(command.Date!.Value, command.StartTime!.Value);

                if (await OverlapsExistingAsync(command.WorkerId, startTimestamp, cancellationToken))
                {
                    return Error.Conflict("Ya existe un marcaje en ese horario.");
                }

                var started = TimeEntry.Start(command.WorkerId, startTimestamp);
                _timeEntryRepository.Add(started);

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

                var pauseTimestamp = Combine(command.Date!.Value, command.StartTime!.Value);

                if (pauseTimestamp <= openEntry.StartedAt)
                {
                    return Error.Validation("La hora de pausa debe ser posterior al inicio de la jornada.");
                }

                openEntry.Pause(pauseTimestamp);

                if (command.EndTime is not null)
                {
                    openEntry.Resume(Combine(command.Date.Value, command.EndTime.Value));
                }

                return TimeEntryResponse.From(openEntry);

            case "end":
                if (openEntry is null)
                {
                    return Error.Conflict("No hay una jornada abierta que finalizar.");
                }

                var endTimestamp = Combine(command.Date!.Value, command.EndTime!.Value);

                if (endTimestamp <= openEntry.StartedAt)
                {
                    return Error.Validation("La hora de final debe ser posterior al inicio de la jornada.");
                }

                openEntry.End(endTimestamp);

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

    private static DateTimeOffset Combine(DateOnly date, TimeOnly time)
    {
        var local = date.ToDateTime(time);
        var offset = MadridTimeZone.Instance.GetUtcOffset(local);

        return new DateTimeOffset(local, offset);
    }
}
