using GestionPersonal.Application.TimeEntries;
using GestionPersonal.Application.Workers;
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

    [Fact]
    public async Task QuickClock_StartWithPreviousDayOpenEntry_ReturnsStaleWarning()
    {
        var workerId = await CreateWorkerAsync(pin: "7004");
        var yesterday = MadridTestTime.Today().AddDays(-1);

        await TimeEntries.AddManualAsync(new AddManualTimeEntryCommand(
            workerId, "start", yesterday, new TimeOnly(9, 0), null));

        var result = await TimeEntries.QuickClockAsync(new QuickClockCommand(workerId, "7004", "start"));

        AssertSuccess(result);
        Assert.False(result.Value.Success);
        Assert.Equal(yesterday, result.Value.StaleEntryDate);
        Assert.Contains("no se finalizó", result.Value.Message);
        Assert.Empty(result.Value.AvailableActions);
    }

    [Fact]
    public async Task QuickClock_EndAt_ClosesPreviousDayEntry()
    {
        var workerId = await CreateWorkerAsync(pin: "7005");
        var yesterday = MadridTestTime.Today().AddDays(-1);

        await TimeEntries.AddManualAsync(new AddManualTimeEntryCommand(
            workerId, "start", yesterday, new TimeOnly(9, 0), null));

        var result = await TimeEntries.QuickClockAsync(new QuickClockCommand(
            workerId, "7005", "end-at", yesterday, new TimeOnly(18, 0)));

        AssertSuccess(result);
        Assert.True(result.Value.Success);

        var startResult = await TimeEntries.QuickClockAsync(new QuickClockCommand(workerId, "7005", "start"));

        AssertSuccess(startResult);
        Assert.True(startResult.Value.Success);
    }

    [Fact]
    public async Task QuickClock_EndAt_BeforeStart_ReturnsValidationError()
    {
        var workerId = await CreateWorkerAsync(pin: "7006");
        var yesterday = MadridTestTime.Today().AddDays(-1);

        await TimeEntries.AddManualAsync(new AddManualTimeEntryCommand(
            workerId, "start", yesterday, new TimeOnly(9, 0), null));

        var result = await TimeEntries.QuickClockAsync(new QuickClockCommand(
            workerId, "7006", "end-at", yesterday, new TimeOnly(8, 0)));

        AssertFailure(result, "Validation");
    }

    [Fact]
    public async Task QuickClock_EndAt_WithoutDateOrTime_ReturnsValidationError()
    {
        var workerId = await CreateWorkerAsync(pin: "7007");

        var result = await TimeEntries.QuickClockAsync(new QuickClockCommand(workerId, "7007", "end-at"));

        AssertFailure(result, "Validation");
    }

    [Fact]
    public async Task QuickClock_WithMustChangePin_RequiresChangeBeforeClocking()
    {
        var workerId = await CreateWorkerAsync(pin: "7008", mustChangePin: true);

        var result = await TimeEntries.QuickClockAsync(new QuickClockCommand(workerId, "7008", "start"));

        AssertSuccess(result);
        Assert.False(result.Value.Success);
        Assert.True(result.Value.MustChangePin);

        var status = await Workers.GetStatusAsync(workerId);
        AssertSuccess(status);
        Assert.Equal("idle", status.Value.CurrentStatus);

        var change = await Workers.ChangePinAsync(workerId, new ChangePinCommand("7008", "7009"));
        AssertSuccess(change);

        var clock = await TimeEntries.QuickClockAsync(new QuickClockCommand(workerId, "7009", "start"));

        AssertSuccess(clock);
        Assert.True(clock.Value.Success);
    }
}
