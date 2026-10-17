using GestionPersonal.Application.TimeEntries;
using GestionPersonal.Tests.Infrastructure;

namespace GestionPersonal.Tests.Services;

public sealed class AddManualTimeEntryTests : ApplicationTestBase, IClassFixture<ApplicationFixture>
{
    public AddManualTimeEntryTests(ApplicationFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task AddManual_Start_NoOpenEntry_Succeeds()
    {
        var workerId = await CreateWorkerAsync();

        var result = await TimeEntries.AddManualAsync(Build(workerId, "start", Yesterday, new TimeOnly(9, 0)));

        AssertSuccess(result);
        Assert.Equal(workerId, result.Value.WorkerId);
        Assert.Equal("in-progress", result.Value.Status);
        Assert.NotEqual(default, result.Value.StartedAt);
    }

    [Fact]
    public async Task AddManual_Start_WhenOpen_ReturnsConflict()
    {
        var workerId = await CreateWorkerAsync();

        await TimeEntries.AddManualAsync(Build(workerId, "start", Yesterday, new TimeOnly(9, 0)));
        var result = await TimeEntries.AddManualAsync(Build(workerId, "start", Yesterday, new TimeOnly(9, 30)));

        AssertFailure(result, "Conflict");
    }

    [Fact]
    public async Task AddManual_Start_OverlappingFinishedEntry_ReturnsConflict()
    {
        var workerId = await CreateWorkerAsync();

        await TimeEntries.AddManualAsync(Build(workerId, "start", Yesterday, new TimeOnly(9, 0)));
        await TimeEntries.AddManualAsync(Build(workerId, "end", Yesterday, endTime: new TimeOnly(18, 0)));

        var result = await TimeEntries.AddManualAsync(Build(workerId, "start", Yesterday, new TimeOnly(10, 0)));

        AssertFailure(result, "Conflict");
    }

    [Fact]
    public async Task AddManual_Pause_NoOpenEntry_ReturnsConflict()
    {
        var workerId = await CreateWorkerAsync();

        var result = await TimeEntries.AddManualAsync(Build(workerId, "pause", Yesterday, new TimeOnly(13, 0)));

        AssertFailure(result, "Conflict");
    }

    [Fact]
    public async Task AddManual_Pause_WithEndGreaterThanStart_Succeeds()
    {
        var workerId = await CreateWorkerAsync();

        await TimeEntries.AddManualAsync(Build(workerId, "start", Yesterday, new TimeOnly(9, 0)));
        var result = await TimeEntries.AddManualAsync(
            Build(workerId, "pause", Yesterday, new TimeOnly(13, 0), new TimeOnly(14, 0)));

        AssertSuccess(result);
        Assert.Equal("in-progress", result.Value.Status);
    }

    [Fact]
    public async Task AddManual_Pause_WithEndNotGreaterThanStart_ReturnsValidationError()
    {
        var workerId = await CreateWorkerAsync();

        await TimeEntries.AddManualAsync(Build(workerId, "start", Yesterday, new TimeOnly(9, 0)));
        var result = await TimeEntries.AddManualAsync(
            Build(workerId, "pause", Yesterday, new TimeOnly(13, 0), new TimeOnly(12, 0)));

        AssertFailure(result, "Validation");
    }

    [Fact]
    public async Task AddManual_End_NoOpenEntry_ReturnsConflict()
    {
        var workerId = await CreateWorkerAsync();

        var result = await TimeEntries.AddManualAsync(Build(workerId, "end", Yesterday, endTime: new TimeOnly(18, 0)));

        AssertFailure(result, "Conflict");
    }

    [Fact]
    public async Task AddManual_End_BeforeStart_ReturnsValidationError()
    {
        var workerId = await CreateWorkerAsync();

        await TimeEntries.AddManualAsync(Build(workerId, "start", Yesterday, new TimeOnly(9, 0)));
        var result = await TimeEntries.AddManualAsync(Build(workerId, "end", Yesterday, endTime: new TimeOnly(8, 0)));

        AssertFailure(result, "Validation");
    }

    [Fact]
    public async Task AddManual_FutureDate_ReturnsValidationError()
    {
        var workerId = await CreateWorkerAsync();

        var result = await TimeEntries.AddManualAsync(
            Build(workerId, "start", MadridTestTime.Today().AddDays(2), new TimeOnly(9, 0)));

        AssertFailure(result, "Validation");
    }

    [Fact]
    public async Task AddManual_InvalidType_ReturnsValidationError()
    {
        var workerId = await CreateWorkerAsync();

        var result = await TimeEntries.AddManualAsync(Build(workerId, "foo", Yesterday, new TimeOnly(9, 0)));

        AssertFailure(result, "Validation");
    }

    [Fact]
    public async Task AddManual_Start_MissingStartTime_ReturnsValidationError()
    {
        var workerId = await CreateWorkerAsync();

        var result = await TimeEntries.AddManualAsync(Build(workerId, "start", Yesterday));

        AssertFailure(result, "Validation");
    }

    [Fact]
    public async Task AddManual_End_MissingEndTime_ReturnsValidationError()
    {
        var workerId = await CreateWorkerAsync();

        var result = await TimeEntries.AddManualAsync(Build(workerId, "end", Yesterday));

        AssertFailure(result, "Validation");
    }

    private static DateOnly Yesterday => MadridTestTime.Today().AddDays(-1);

    private static AddManualTimeEntryCommand Build(
        int workerId,
        string type,
        DateOnly date,
        TimeOnly? startTime = null,
        TimeOnly? endTime = null) =>
        new(workerId, type, date, startTime, endTime);
}
