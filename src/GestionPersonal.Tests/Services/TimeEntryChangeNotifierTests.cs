using GestionPersonal.Application.Common;

namespace GestionPersonal.Tests.Services;

public sealed class TimeEntryChangeNotifierTests
{
    [Fact]
    public void Raise_InvokesSubscribedHandler_WithCenterId()
    {
        var notifier = new TimeEntryChangeNotifier();
        var received = new List<int>();
        using var subscription = notifier.Subscribe(received.Add);

        notifier.Raise(42);

        Assert.Equal([42], received);
    }

    [Fact]
    public void Raise_InvokesEverySubscribedHandler()
    {
        var notifier = new TimeEntryChangeNotifier();
        var first = 0;
        var second = 0;
        using var firstSubscription = notifier.Subscribe(id => first = id);
        using var secondSubscription = notifier.Subscribe(id => second = id);

        notifier.Raise(7);

        Assert.Equal(7, first);
        Assert.Equal(7, second);
    }

    [Fact]
    public void Dispose_UnsubscribesHandler()
    {
        var notifier = new TimeEntryChangeNotifier();
        var received = new List<int>();
        var subscription = notifier.Subscribe(received.Add);

        subscription.Dispose();
        notifier.Raise(42);

        Assert.Empty(received);
    }

    [Fact]
    public void Raise_WhenHandlerThrows_StillInvokesOtherHandlers()
    {
        var notifier = new TimeEntryChangeNotifier();
        using var failing = notifier.Subscribe(_ => throw new InvalidOperationException());
        var received = 0;
        using var working = notifier.Subscribe(id => received = id);

        notifier.Raise(9);

        Assert.Equal(9, received);
    }

    [Fact]
    public void Dispose_IsIdempotent()
    {
        var notifier = new TimeEntryChangeNotifier();
        var subscription = notifier.Subscribe(_ => { });

        subscription.Dispose();
        subscription.Dispose();
    }
}
