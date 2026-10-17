using GestionPersonal.Application.TimeEntries;
using GestionPersonal.Tests.Infrastructure;

namespace GestionPersonal.Tests.Services;

public sealed class ResumeTimeEntryTests : ApplicationTestBase, IClassFixture<ApplicationFixture>
{
    public ResumeTimeEntryTests(ApplicationFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task Resume_WhenPaused_ReturnsOk()
    {
        var workerId = await CreateWorkerAsync();

        await TimeEntries.StartAsync(new StartTimeEntryCommand(workerId));
        await TimeEntries.PauseAsync(new PauseTimeEntryCommand(workerId));
        var result = await TimeEntries.ResumeAsync(new ResumeTimeEntryCommand(workerId));

        AssertSuccess(result);
        Assert.Equal("in-progress", result.Value.Status);

        var pause = Assert.Single(result.Value.Pauses);
        Assert.NotNull(pause.ResumedAt);
        Assert.True(pause.ResumedAt.Value >= pause.PausedAt);
    }

    [Fact]
    public async Task Resume_WhenNotStarted_ReturnsConflict()
    {
        var workerId = await CreateWorkerAsync();

        var result = await TimeEntries.ResumeAsync(new ResumeTimeEntryCommand(workerId));

        AssertFailure(result, "Conflict");
    }

    [Fact]
    public async Task Resume_WhenInProgress_ReturnsConflict()
    {
        var workerId = await CreateWorkerAsync();

        await TimeEntries.StartAsync(new StartTimeEntryCommand(workerId));
        var result = await TimeEntries.ResumeAsync(new ResumeTimeEntryCommand(workerId));

        AssertFailure(result, "Conflict");
    }

    [Fact]
    public async Task Resume_WithEmptyWorkerId_ReturnsValidationError()
    {
        var result = await TimeEntries.ResumeAsync(new ResumeTimeEntryCommand(0));

        AssertFailure(result, "Validation");
    }

    [Fact]
    public async Task FullCycle_TracksMultiplePausesWithDates()
    {
        var workerId = await CreateWorkerAsync();

        await TimeEntries.StartAsync(new StartTimeEntryCommand(workerId));
        await TimeEntries.PauseAsync(new PauseTimeEntryCommand(workerId));
        await TimeEntries.ResumeAsync(new ResumeTimeEntryCommand(workerId));
        await TimeEntries.PauseAsync(new PauseTimeEntryCommand(workerId));
        var result = await TimeEntries.EndAsync(new EndTimeEntryCommand(workerId));

        AssertSuccess(result);
        Assert.Equal("finished", result.Value.Status);
        Assert.Equal(2, result.Value.Pauses.Count);

        foreach (var pause in result.Value.Pauses)
        {
            Assert.True(pause.ResumedAt.HasValue);
            Assert.True(pause.ResumedAt.Value >= pause.PausedAt);
        }
    }
}
