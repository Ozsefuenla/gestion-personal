using GestionPersonal.Application.TimeEntries;
using GestionPersonal.Tests.Infrastructure;

namespace GestionPersonal.Tests.Services;

public sealed class WorkersStatusTests : ApplicationTestBase, IClassFixture<ApplicationFixture>
{
    public WorkersStatusTests(ApplicationFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task GetStatuses_ReturnsAllWorkers()
    {
        var result = await Workers.GetStatusesAsync();

        AssertSuccess(result);
        Assert.True(result.Value.Count >= 3);
        Assert.All(result.Value, s =>
        {
            Assert.False(string.IsNullOrWhiteSpace(s.CurrentStatus));
            Assert.Equal(TimeSpan.FromHours(8), s.DailyHours);
        });
    }

    [Fact]
    public async Task GetStatuses_ReflectsStatusTransitions()
    {
        var workerId = await CreateWorkerAsync(fullName: "Transition Worker");

        var status = await GetStatusAsync(workerId);
        Assert.Equal("idle", status.CurrentStatus);
        Assert.Null(status.StartedAt);
        Assert.Equal(TimeSpan.Zero, status.WorkedTime);
        Assert.Empty(status.Timeline);

        await TimeEntries.StartAsync(new StartTimeEntryCommand(workerId));
        status = await GetStatusAsync(workerId);
        Assert.Equal("in-progress", status.CurrentStatus);
        Assert.NotNull(status.StartedAt);
        Assert.Equal("working", Assert.Single(status.Timeline).Type);

        await TimeEntries.PauseAsync(new PauseTimeEntryCommand(workerId));
        status = await GetStatusAsync(workerId);
        Assert.Equal("paused", status.CurrentStatus);
        Assert.NotNull(status.CurrentPausedAt);
        Assert.Equal(new[] { "working", "paused" }, status.Timeline.Select(t => t.Type));

        await TimeEntries.EndAsync(new EndTimeEntryCommand(workerId));
        status = await GetStatusAsync(workerId);
        Assert.Equal("finished", status.CurrentStatus);
        Assert.Equal(new[] { "working", "paused" }, status.Timeline.Select(t => t.Type));
    }

    private async Task<GestionPersonal.Application.Workers.WorkerStatusResponse> GetStatusAsync(int workerId)
    {
        var result = await Workers.GetStatusAsync(workerId);
        AssertSuccess(result);
        return result.Value;
    }
}
