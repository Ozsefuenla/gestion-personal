using GestionPersonal.Application.TimeEntries;
using GestionPersonal.Tests.Infrastructure;

namespace GestionPersonal.Tests.Services;

public sealed class CloseStaleTimeEntryTests : ApplicationTestBase, IClassFixture<ApplicationFixture>
{
    public CloseStaleTimeEntryTests(ApplicationFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task CloseStale_AsWorkerSelf_ClosesPreviousDayEntry()
    {
        var workerId = await CreateWorkerAsync(pin: "6001");
        var yesterday = MadridTestTime.Today().AddDays(-1);

        await TimeEntries.AddManualAsync(new AddManualTimeEntryCommand(
            workerId, "start", yesterday, new TimeOnly(9, 0), null));

        User.ActAsWorker(workerId);

        var result = await TimeEntries.CloseStaleAsync(new CloseStaleTimeEntryCommand(
            workerId, yesterday, new TimeOnly(18, 0)));

        AssertSuccess(result);

        var startResult = await TimeEntries.StartAsync(new StartTimeEntryCommand(workerId));
        AssertSuccess(startResult);
    }

    [Fact]
    public async Task CloseStale_AsCenterAdmin_ClosesWorkerEntry()
    {
        var workerId = await CreateWorkerAsync(pin: "6002");
        var yesterday = MadridTestTime.Today().AddDays(-1);

        await TimeEntries.AddManualAsync(new AddManualTimeEntryCommand(
            workerId, "start", yesterday, new TimeOnly(9, 0), null));

        var result = await TimeEntries.CloseStaleAsync(new CloseStaleTimeEntryCommand(
            workerId, yesterday, new TimeOnly(18, 0)));

        AssertSuccess(result);
    }

    [Fact]
    public async Task CloseStale_AsWorkerSelf_ForAnotherWorker_ReturnsForbidden()
    {
        var workerId = await CreateWorkerAsync(pin: "6003");
        var otherWorkerId = await CreateWorkerAsync(pin: "6004", fullName: "Other Worker");
        var yesterday = MadridTestTime.Today().AddDays(-1);

        await TimeEntries.AddManualAsync(new AddManualTimeEntryCommand(
            otherWorkerId, "start", yesterday, new TimeOnly(9, 0), null));

        User.ActAsWorker(workerId);

        var result = await TimeEntries.CloseStaleAsync(new CloseStaleTimeEntryCommand(
            otherWorkerId, yesterday, new TimeOnly(18, 0)));

        AssertFailure(result, "Forbidden");
    }

    [Fact]
    public async Task CloseStale_WithoutOpenEntry_ReturnsConflict()
    {
        var workerId = await CreateWorkerAsync(pin: "6005");
        var yesterday = MadridTestTime.Today().AddDays(-1);

        User.ActAsWorker(workerId);

        var result = await TimeEntries.CloseStaleAsync(new CloseStaleTimeEntryCommand(
            workerId, yesterday, new TimeOnly(18, 0)));

        AssertFailure(result, "Conflict");
    }
}
