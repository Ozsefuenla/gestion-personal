using GestionPersonal.Application.TimeEntries;
using GestionPersonal.Tests.Infrastructure;

namespace GestionPersonal.Tests.Services;

public sealed class TodaySummaryTests : ApplicationTestBase, IClassFixture<ApplicationFixture>
{
    public TodaySummaryTests(ApplicationFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task GetToday_WhenIdle_ReturnsEmptySummary()
    {
        var workerId = await CreateWorkerAsync();

        var result = await TimeEntries.GetTodayAsync(new TodaySummaryQuery(workerId));

        AssertSuccess(result);
        Assert.Equal("idle", result.Value.CurrentStatus);
        Assert.Null(result.Value.StartedAt);
        Assert.Null(result.Value.CurrentPausedAt);
        Assert.Equal(TimeSpan.Zero, result.Value.WorkedTime);
        Assert.Equal(TimeSpan.Zero, result.Value.PausedTime);
        Assert.Equal(0, result.Value.PauseCount);
        Assert.Equal(0, result.Value.CompletedEntries);
        Assert.Empty(result.Value.Timeline);
    }

    [Fact]
    public async Task GetToday_WhenInProgress_ReturnsWorkingStatus()
    {
        var workerId = await CreateWorkerAsync();

        await TimeEntries.StartAsync(new StartTimeEntryCommand(workerId));
        var result = await TimeEntries.GetTodayAsync(new TodaySummaryQuery(workerId));

        AssertSuccess(result);
        Assert.Equal("in-progress", result.Value.CurrentStatus);
        Assert.NotNull(result.Value.StartedAt);
        Assert.Null(result.Value.CurrentPausedAt);
        Assert.True(result.Value.WorkedTime >= TimeSpan.Zero);
        Assert.Contains(result.Value.Timeline, s => s.Type == "working");
    }

    [Fact]
    public async Task GetToday_WhenPaused_ReturnsPausedStatus()
    {
        var workerId = await CreateWorkerAsync();

        await TimeEntries.StartAsync(new StartTimeEntryCommand(workerId));
        await TimeEntries.PauseAsync(new PauseTimeEntryCommand(workerId));
        var result = await TimeEntries.GetTodayAsync(new TodaySummaryQuery(workerId));

        AssertSuccess(result);
        Assert.Equal("paused", result.Value.CurrentStatus);
        Assert.NotNull(result.Value.CurrentPausedAt);
        Assert.True(result.Value.PausedTime >= TimeSpan.Zero);
        Assert.Contains(result.Value.Timeline, s => s.Type == "paused");
    }

    [Fact]
    public async Task GetToday_FullCycle_ReturnsTimelineWithMultiplePauses()
    {
        var workerId = await CreateWorkerAsync();

        await TimeEntries.StartAsync(new StartTimeEntryCommand(workerId));
        await TimeEntries.PauseAsync(new PauseTimeEntryCommand(workerId));
        await TimeEntries.ResumeAsync(new ResumeTimeEntryCommand(workerId));
        await TimeEntries.PauseAsync(new PauseTimeEntryCommand(workerId));
        await TimeEntries.ResumeAsync(new ResumeTimeEntryCommand(workerId));
        await TimeEntries.EndAsync(new EndTimeEntryCommand(workerId));

        var result = await TimeEntries.GetTodayAsync(new TodaySummaryQuery(workerId));

        AssertSuccess(result);
        Assert.Equal("finished", result.Value.CurrentStatus);
        Assert.Equal(2, result.Value.PauseCount);
        Assert.Equal(1, result.Value.CompletedEntries);
        Assert.Equal(new[] { "working", "paused", "working", "paused", "working" }, result.Value.Timeline.Select(t => t.Type));
    }

    [Fact]
    public async Task GetToday_WithEmptyWorkerId_ReturnsValidationError()
    {
        var result = await TimeEntries.GetTodayAsync(new TodaySummaryQuery(0));

        AssertFailure(result, "Validation");
    }
}
