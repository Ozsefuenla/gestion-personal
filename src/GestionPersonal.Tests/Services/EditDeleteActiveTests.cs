using GestionPersonal.Application.TimeEntries;
using GestionPersonal.Application.Workers;
using GestionPersonal.Tests.Infrastructure;

namespace GestionPersonal.Tests.Services;

public sealed class EditDeleteActiveTests : ApplicationTestBase, IClassFixture<ApplicationFixture>
{
    public EditDeleteActiveTests(ApplicationFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task EditActive_WhenInProgress_Succeeds()
    {
        var workerId = await CreateWorkerAsync();
        await TimeEntries.StartAsync(new StartTimeEntryCommand(workerId));

        var newStart = DateTimeOffset.UtcNow.AddMinutes(-10);
        var result = await TimeEntries.EditActiveAsync(workerId, new EditActiveTimeEntryCommand(newStart));

        AssertSuccess(result);
    }

    [Fact]
    public async Task EditActive_WhenPaused_Succeeds()
    {
        var workerId = await CreateWorkerAsync();
        await TimeEntries.StartAsync(new StartTimeEntryCommand(workerId));
        await Task.Delay(100);
        await TimeEntries.PauseAsync(new PauseTimeEntryCommand(workerId));
        await Task.Delay(100);

        var newStart = DateTimeOffset.UtcNow.AddMilliseconds(-50);
        var result = await TimeEntries.EditActiveAsync(workerId, new EditActiveTimeEntryCommand(newStart));

        AssertSuccess(result);
    }

    [Fact]
    public async Task EditActive_WithFutureStart_ReturnsValidationError()
    {
        var workerId = await CreateWorkerAsync();
        await TimeEntries.StartAsync(new StartTimeEntryCommand(workerId));

        var futureStart = DateTimeOffset.UtcNow.AddMinutes(10);
        var result = await TimeEntries.EditActiveAsync(workerId, new EditActiveTimeEntryCommand(futureStart));

        AssertFailure(result, "Validation");
    }

    [Fact]
    public async Task EditActive_WithNoOpenEntry_ReturnsValidationError()
    {
        var workerId = await CreateWorkerAsync();

        var result = await TimeEntries.EditActiveAsync(workerId, new EditActiveTimeEntryCommand(DateTimeOffset.UtcNow));

        AssertFailure(result, "Validation");
    }

    [Fact]
    public async Task DeleteActive_WhenPaused_ReturnsToInProgress()
    {
        var workerId = await CreateWorkerAsync();
        await TimeEntries.StartAsync(new StartTimeEntryCommand(workerId));
        await TimeEntries.PauseAsync(new PauseTimeEntryCommand(workerId));

        var result = await TimeEntries.DeleteActiveAsync(workerId);

        AssertSuccess(result);

        var status = await GetStatusAsync(workerId);
        Assert.Equal("in-progress", status.CurrentStatus);
    }

    [Fact]
    public async Task DeleteActive_WhenInProgressNoPauses_ReturnsIdle()
    {
        var workerId = await CreateWorkerAsync();
        await TimeEntries.StartAsync(new StartTimeEntryCommand(workerId));

        var result = await TimeEntries.DeleteActiveAsync(workerId);

        AssertSuccess(result);

        var status = await GetStatusAsync(workerId);
        Assert.Equal("idle", status.CurrentStatus);
    }

    [Fact]
    public async Task DeleteActive_WithNoOpenEntry_ReturnsValidationError()
    {
        var workerId = await CreateWorkerAsync();

        var result = await TimeEntries.DeleteActiveAsync(workerId);

        AssertFailure(result, "Validation");
    }

    private async Task<WorkerStatusResponse> GetStatusAsync(int workerId)
    {
        var result = await Workers.GetStatusAsync(workerId);
        AssertSuccess(result);
        return result.Value;
    }
}
