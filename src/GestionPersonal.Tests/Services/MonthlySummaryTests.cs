using GestionPersonal.Application.TimeEntries;
using GestionPersonal.Tests.Infrastructure;

namespace GestionPersonal.Tests.Services;

public sealed class MonthlySummaryTests : ApplicationTestBase, IClassFixture<ApplicationFixture>
{
    public MonthlySummaryTests(ApplicationFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task GetMonthly_WithNoEntries_ReturnsAllDaysEmpty()
    {
        var workerId = await CreateWorkerAsync();
        var (year, month) = MadridTestTime.CurrentMonth();

        var result = await TimeEntries.GetMonthlyAsync(new MonthlySummaryQuery(workerId, year, month));

        AssertSuccess(result);
        Assert.Equal(year, result.Value.Year);
        Assert.Equal(month, result.Value.Month);
        Assert.Equal(TimeSpan.FromHours(8), result.Value.DailyTarget);
        Assert.Equal(DateTime.DaysInMonth(year, month), result.Value.Days.Count);
        Assert.All(result.Value.Days, d =>
        {
            Assert.Equal(TimeSpan.Zero, d.WorkedTime);
            Assert.Equal(TimeSpan.Zero, d.PausedTime);
            Assert.False(d.HasEntries);
            Assert.Empty(d.Timeline);
        });
    }

    [Fact]
    public async Task GetMonthly_AfterStartEnd_TodayHasWorkedTime()
    {
        var workerId = await CreateWorkerAsync();
        var (year, month) = MadridTestTime.CurrentMonth();

        await TimeEntries.StartAsync(new StartTimeEntryCommand(workerId));
        await TimeEntries.EndAsync(new EndTimeEntryCommand(workerId));

        var result = await TimeEntries.GetMonthlyAsync(new MonthlySummaryQuery(workerId, year, month));

        AssertSuccess(result);
        var activeDay = result.Value.Days.Single(d => d.HasEntries);
        Assert.True(activeDay.WorkedTime > TimeSpan.Zero);
        Assert.False(activeDay.HasOpenEntry);
    }

    [Fact]
    public async Task GetMonthly_AfterStartOnly_TodayHasOpenEntry()
    {
        var workerId = await CreateWorkerAsync();
        var (year, month) = MadridTestTime.CurrentMonth();

        await TimeEntries.StartAsync(new StartTimeEntryCommand(workerId));

        var result = await TimeEntries.GetMonthlyAsync(new MonthlySummaryQuery(workerId, year, month));

        AssertSuccess(result);
        var activeDay = result.Value.Days.Single(d => d.HasEntries);
        Assert.True(activeDay.HasOpenEntry);
    }

    [Fact]
    public async Task GetMonthly_AfterFullCycle_TodayHasPausedTimeAndTimeline()
    {
        var workerId = await CreateWorkerAsync();
        var (year, month) = MadridTestTime.CurrentMonth();

        await TimeEntries.StartAsync(new StartTimeEntryCommand(workerId));
        await TimeEntries.PauseAsync(new PauseTimeEntryCommand(workerId));
        await TimeEntries.ResumeAsync(new ResumeTimeEntryCommand(workerId));
        await TimeEntries.EndAsync(new EndTimeEntryCommand(workerId));

        var result = await TimeEntries.GetMonthlyAsync(new MonthlySummaryQuery(workerId, year, month));

        AssertSuccess(result);
        var activeDay = result.Value.Days.Single(d => d.HasEntries);
        Assert.True(activeDay.WorkedTime > TimeSpan.Zero);
        Assert.True(activeDay.PausedTime > TimeSpan.Zero);
        Assert.Contains(activeDay.Timeline, s => s.Type == "working");
        Assert.Contains(activeDay.Timeline, s => s.Type == "paused");
    }

    [Fact]
    public async Task GetMonthly_WithInvalidMonth_ReturnsValidationError()
    {
        var workerId = await CreateWorkerAsync();
        var (year, _) = MadridTestTime.CurrentMonth();

        var result = await TimeEntries.GetMonthlyAsync(new MonthlySummaryQuery(workerId, year, 13));

        AssertFailure(result, "Validation");
    }

    [Fact]
    public async Task GetMonthly_WithUnknownWorker_ReturnsNotFound()
    {
        var (year, month) = MadridTestTime.CurrentMonth();

        var result = await TimeEntries.GetMonthlyAsync(new MonthlySummaryQuery(TestWorkerIds.New(), year, month));

        AssertFailure(result, "NotFound");
    }
}
