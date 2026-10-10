using GestionPersonal.Application.Abstractions;
using GestionPersonal.Application.Common;

namespace GestionPersonal.Application.TimeEntries;

public sealed class MonthlySummaryQueryHandler
{
    private readonly IWorkerRepository _workerRepository;
    private readonly ITimeEntryRepository _timeEntryRepository;

    public MonthlySummaryQueryHandler(
        IWorkerRepository workerRepository,
        ITimeEntryRepository timeEntryRepository)
    {
        _workerRepository = workerRepository;
        _timeEntryRepository = timeEntryRepository;
    }

    public async Task<Result<MonthlySummaryResponse>> Handle(
        MonthlySummaryQuery query,
        CancellationToken cancellationToken = default)
    {
        var worker = await _workerRepository.GetByIdAsync(query.WorkerId, cancellationToken);

        if (worker is null)
        {
            return Error.NotFound("El trabajador no existe.");
        }

        var now = DateTimeOffset.UtcNow;
        var (from, to) = MonthlySummaryHelper.GetMonthRange(query.Year, query.Month);
        var entries = await _timeEntryRepository.GetByWorkerOverlappingAsync(query.WorkerId, from, to, cancellationToken);

        var totalDays = MonthlySummaryHelper.DaysInMonth(query.Year, query.Month);
        var days = new List<DailySummaryResponse>(totalDays);

        for (var day = 1; day <= totalDays; day++)
        {
            var (dayFrom, dayTo) = MonthlySummaryHelper.GetDayRange(query.Year, query.Month, day);
            var worked = TimeSpan.Zero;
            var paused = TimeSpan.Zero;
            var timeline = new List<TimelineSegmentResponse>();
            var hasEntries = false;

            foreach (var entry in entries)
            {
                var entryEnd = entry.EndedAt ?? now;

                if (entry.StartedAt < dayTo && entryEnd > dayFrom)
                {
                    hasEntries = true;
                }

                foreach (var segment in MonthlySummaryHelper.GetDayTimeline(entry, dayFrom, dayTo, now))
                {
                    timeline.Add(new TimelineSegmentResponse(segment.Type, segment.From, segment.To));

                    if (segment.Type == "working")
                    {
                        worked += segment.To - segment.From;
                    }
                    else
                    {
                        paused += segment.To - segment.From;
                    }
                }
            }

            days.Add(new DailySummaryResponse(day, worked, paused, hasEntries, timeline));
        }

        return new MonthlySummaryResponse(query.Year, query.Month, worker.DailyHours, days);
    }
}
