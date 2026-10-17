using GestionPersonal.Application.TimeEntries;
using GestionPersonal.Tests.Infrastructure;

namespace GestionPersonal.Tests.Services;

public sealed class EndTimeEntryTests : ApplicationTestBase, IClassFixture<ApplicationFixture>
{
    public EndTimeEntryTests(ApplicationFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task End_WhenInProgress_ReturnsOk()
    {
        var workerId = await CreateWorkerAsync();

        await TimeEntries.StartAsync(new StartTimeEntryCommand(workerId));
        var result = await TimeEntries.EndAsync(new EndTimeEntryCommand(workerId));

        AssertSuccess(result);
        Assert.Equal("finished", result.Value.Status);
        Assert.NotNull(result.Value.EndedAt);
    }

    [Fact]
    public async Task End_WhenNotStarted_ReturnsConflict()
    {
        var workerId = await CreateWorkerAsync();

        var result = await TimeEntries.EndAsync(new EndTimeEntryCommand(workerId));

        AssertFailure(result, "Conflict");
    }

    [Fact]
    public async Task End_WhenPaused_ReturnsOk()
    {
        var workerId = await CreateWorkerAsync();

        await TimeEntries.StartAsync(new StartTimeEntryCommand(workerId));
        await TimeEntries.PauseAsync(new PauseTimeEntryCommand(workerId));
        var result = await TimeEntries.EndAsync(new EndTimeEntryCommand(workerId));

        AssertSuccess(result);
        Assert.Equal("finished", result.Value.Status);
        Assert.NotNull(result.Value.EndedAt);

        var pause = Assert.Single(result.Value.Pauses);
        Assert.NotNull(pause.ResumedAt);
    }

    [Fact]
    public async Task End_WithEmptyWorkerId_ReturnsValidationError()
    {
        var result = await TimeEntries.EndAsync(new EndTimeEntryCommand(0));

        AssertFailure(result, "Validation");
    }
}
