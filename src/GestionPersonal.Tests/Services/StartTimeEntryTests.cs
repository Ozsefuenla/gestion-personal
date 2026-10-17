using GestionPersonal.Application.TimeEntries;
using GestionPersonal.Tests.Infrastructure;

namespace GestionPersonal.Tests.Services;

public sealed class StartTimeEntryTests : ApplicationTestBase, IClassFixture<ApplicationFixture>
{
    public StartTimeEntryTests(ApplicationFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task Start_WithNoOpenEntry_ReturnsOk()
    {
        var workerId = await CreateWorkerAsync();

        var result = await TimeEntries.StartAsync(new StartTimeEntryCommand(workerId));

        AssertSuccess(result);
        Assert.NotEqual(Guid.Empty, result.Value.Id);
        Assert.Equal(workerId, result.Value.WorkerId);
        Assert.Equal("in-progress", result.Value.Status);
        Assert.NotEqual(default, result.Value.StartedAt);
    }

    [Fact]
    public async Task Start_WhenAlreadyStarted_ReturnsConflict()
    {
        var workerId = await CreateWorkerAsync();

        await TimeEntries.StartAsync(new StartTimeEntryCommand(workerId));
        var result = await TimeEntries.StartAsync(new StartTimeEntryCommand(workerId));

        AssertFailure(result, "Conflict");
    }

    [Fact]
    public async Task Start_WithEmptyWorkerId_ReturnsValidationError()
    {
        var result = await TimeEntries.StartAsync(new StartTimeEntryCommand(0));

        AssertFailure(result, "Validation");
    }
}
