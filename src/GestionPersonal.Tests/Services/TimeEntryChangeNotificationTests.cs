using GestionPersonal.Application.TimeEntries;
using GestionPersonal.Tests.Infrastructure;

namespace GestionPersonal.Tests.Services;

public sealed class TimeEntryChangeNotificationTests : ApplicationTestBase, IClassFixture<ApplicationFixture>
{
    public TimeEntryChangeNotificationTests(ApplicationFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task Start_WhenSuccessful_RaisesChangeForCurrentCenter()
    {
        var workerId = await CreateWorkerAsync();
        var received = new List<int>();
        using var subscription = Notifier.Subscribe(received.Add);

        var result = await TimeEntries.StartAsync(new StartTimeEntryCommand(workerId));

        AssertSuccess(result);
        Assert.Equal([ApplicationFixture.TestCenterId], received);
    }

    [Fact]
    public async Task Start_WhenConflict_DoesNotRaise()
    {
        var workerId = await CreateWorkerAsync();
        AssertSuccess(await TimeEntries.StartAsync(new StartTimeEntryCommand(workerId)));

        var received = new List<int>();
        using var subscription = Notifier.Subscribe(received.Add);

        var result = await TimeEntries.StartAsync(new StartTimeEntryCommand(workerId));

        AssertFailure(result, "Conflict");
        Assert.Empty(received);
    }

    [Fact]
    public async Task End_WhenSuccessful_RaisesChange()
    {
        var workerId = await CreateWorkerAsync();
        AssertSuccess(await TimeEntries.StartAsync(new StartTimeEntryCommand(workerId)));

        var received = new List<int>();
        using var subscription = Notifier.Subscribe(received.Add);

        var result = await TimeEntries.EndAsync(new EndTimeEntryCommand(workerId));

        AssertSuccess(result);
        Assert.Equal([ApplicationFixture.TestCenterId], received);
    }

    [Fact]
    public async Task QuickClock_WhenActionNotAllowed_DoesNotRaise()
    {
        var workerId = await CreateWorkerAsync(pin: "7101");
        AssertSuccess(await TimeEntries.QuickClockAsync(new QuickClockCommand(workerId, "7101", "start")));

        var received = new List<int>();
        using var subscription = Notifier.Subscribe(received.Add);

        var result = await TimeEntries.QuickClockAsync(new QuickClockCommand(workerId, "7101", "start"));

        AssertSuccess(result);
        Assert.False(result.Value.Success);
        Assert.Empty(received);
    }
}
