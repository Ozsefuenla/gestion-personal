using GestionPersonal.Application.TimeEntries;
using GestionPersonal.Tests.Infrastructure;

namespace GestionPersonal.Tests.Services;

public sealed class PauseTimeEntryTests : ApplicationTestBase, IClassFixture<ApplicationFixture>
{
    public PauseTimeEntryTests(ApplicationFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task Pause_WhenInProgress_ReturnsOk()
    {
        var workerId = await CreateWorkerAsync();

        await TimeEntries.StartAsync(new StartTimeEntryCommand(workerId));
        var result = await TimeEntries.PauseAsync(new PauseTimeEntryCommand(workerId));

        AssertSuccess(result);
        Assert.Equal("paused", result.Value.Status);

        var pause = Assert.Single(result.Value.Pauses);
        Assert.NotEqual(default, pause.PausedAt);
        Assert.Null(pause.ResumedAt);
    }

    [Fact]
    public async Task Pause_WhenNotStarted_ReturnsConflict()
    {
        var workerId = await CreateWorkerAsync();

        var result = await TimeEntries.PauseAsync(new PauseTimeEntryCommand(workerId));

        AssertFailure(result, "Conflict");
    }

    [Fact]
    public async Task Pause_WhenAlreadyPaused_ReturnsConflict()
    {
        var workerId = await CreateWorkerAsync();

        await TimeEntries.StartAsync(new StartTimeEntryCommand(workerId));
        await TimeEntries.PauseAsync(new PauseTimeEntryCommand(workerId));
        var result = await TimeEntries.PauseAsync(new PauseTimeEntryCommand(workerId));

        AssertFailure(result, "Conflict");
    }

    [Fact]
    public async Task Pause_WithEmptyWorkerId_ReturnsValidationError()
    {
        var result = await TimeEntries.PauseAsync(new PauseTimeEntryCommand(0));

        AssertFailure(result, "Validation");
    }
}
