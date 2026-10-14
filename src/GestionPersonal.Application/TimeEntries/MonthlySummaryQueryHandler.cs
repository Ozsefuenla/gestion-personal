using GestionPersonal.Application.Abstractions;
using GestionPersonal.Application.Common;
using GestionPersonal.Application.Vacations;
using GestionPersonal.Domain.Entities;

namespace GestionPersonal.Application.TimeEntries;

public sealed class MonthlySummaryQueryHandler
{
    private readonly IWorkerRepository _workerRepository;
    private readonly ITimeEntryRepository _timeEntryRepository;
    private readonly IVacationRepository _vacationRepository;
    private readonly ICurrentCenter _currentCenter;

    public MonthlySummaryQueryHandler(
        IWorkerRepository workerRepository,
        ITimeEntryRepository timeEntryRepository,
        IVacationRepository vacationRepository,
        ICurrentCenter currentCenter)
    {
        _workerRepository = workerRepository;
        _timeEntryRepository = timeEntryRepository;
        _vacationRepository = vacationRepository;
        _currentCenter = currentCenter;
    }

    public async Task<Result<MonthlySummaryResponse>> Handle(
        MonthlySummaryQuery query,
        CancellationToken cancellationToken = default)
    {
        var workerResult = await WorkerScope.GetScopedWorkerAsync(_workerRepository, _currentCenter, query.WorkerId, cancellationToken);

        if (workerResult.IsFailure)
        {
            return workerResult.Error;
        }

        var worker = workerResult.Value;

        var now = DateTimeOffset.UtcNow;
        var (from, to) = MonthlySummaryHelper.GetMonthRange(query.Year, query.Month);
        var entries = await _timeEntryRepository.GetByWorkerOverlappingAsync(query.WorkerId, from, to, cancellationToken);

        var monthStart = new DateOnly(query.Year, query.Month, 1);
        var monthEnd = monthStart.AddDays(MonthlySummaryHelper.DaysInMonth(query.Year, query.Month) - 1);
        var approvedVacations = await _vacationRepository.GetApprovedByWorkerBetweenAsync(query.WorkerId, monthStart, monthEnd, cancellationToken);

        var absenceByDay = BuildAbsenceMap(approvedVacations, monthStart, monthEnd);

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

            var date = new DateOnly(query.Year, query.Month, day);
            var isAbsence = absenceByDay.TryGetValue(date, out var absenceType);

            days.Add(new DailySummaryResponse(day, worked, paused, hasEntries, isAbsence, absenceType, timeline));
        }

        return new MonthlySummaryResponse(query.Year, query.Month, worker.DailyHours, days);
    }

    private static Dictionary<DateOnly, string> BuildAbsenceMap(
        IReadOnlyList<Vacation> vacations,
        DateOnly monthStart,
        DateOnly monthEnd)
    {
        var map = new Dictionary<DateOnly, string>();

        foreach (var vacation in vacations)
        {
            var start = vacation.StartDate < monthStart ? monthStart : vacation.StartDate;
            var end = vacation.EndDate > monthEnd ? monthEnd : vacation.EndDate;

            for (var date = start; date <= end; date = date.AddDays(1))
            {
                map[date] = VacationTypeHelper.Format(vacation.Type);
            }
        }

        return map;
    }
}
