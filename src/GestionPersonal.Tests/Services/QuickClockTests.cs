using GestionPersonal.Application.TimeEntries;
using GestionPersonal.Tests.Infrastructure;

namespace GestionPersonal.Tests.Services;

public sealed class QuickClockTests : ApplicationTestBase, IClassFixture<ApplicationFixture>
{
    public QuickClockTests(ApplicationFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task QuickClock_WithInvalidPinFormat_ReturnsValidationError()
    {
        var result = await TimeEntries.QuickClockAsync(new QuickClockCommand(1, "12ab", "start"));

        AssertFailure(result, "Validation");
    }

    [Fact]
    public async Task QuickClock_WithUnknownPin_ReturnsValidationError()
    {
        var result = await TimeEntries.QuickClockAsync(new QuickClockCommand(1, "5555", "start"));

        AssertFailure(result, "Validation");
    }

    [Fact]
    public async Task QuickClock_WithUnknownWorker_ReturnsNotFound()
    {
        var result = await TimeEntries.QuickClockAsync(new QuickClockCommand(TestWorkerIds.New(), "1111", "start"));

        AssertFailure(result, "NotFound");
    }

    [Fact]
    public async Task QuickClock_StartWhenIdle_Succeeds()
    {
        var workerId = await CreateWorkerAsync(pin: "7001");

        var result = await TimeEntries.QuickClockAsync(new QuickClockCommand(workerId, "7001", "start"));

        AssertSuccess(result);
        Assert.True(result.Value.Success);
    }

    [Fact]
    public async Task QuickClock_StartWhenInProgress_ReturnsAvailableActions()
    {
        var workerId = await CreateWorkerAsync(pin: "7002");

        await TimeEntries.QuickClockAsync(new QuickClockCommand(workerId, "7002", "start"));
        var result = await TimeEntries.QuickClockAsync(new QuickClockCommand(workerId, "7002", "start"));

        AssertSuccess(result);
        Assert.False(result.Value.Success);
        Assert.Equal("in-progress", result.Value.CurrentStatus);
        Assert.Equal(new[] { "pause", "end" }, result.Value.AvailableActions);
    }

    [Fact]
    public async Task QuickClock_StartActsAsResume_WhenPaused()
    {
        var workerId = await CreateWorkerAsync(pin: "7003");

        await TimeEntries.QuickClockAsync(new QuickClockCommand(workerId, "7003", "start"));
        await TimeEntries.QuickClockAsync(new QuickClockCommand(workerId, "7003", "pause"));

        var result = await TimeEntries.QuickClockAsync(new QuickClockCommand(workerId, "7003", "start"));

        AssertSuccess(result);
        Assert.True(result.Value.Success);
    }
}
