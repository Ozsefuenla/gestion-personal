using GestionPersonal.Application.Abstractions;

namespace GestionPersonal.Application.Common;

public sealed class TimeEntryChangeNotifier : ITimeEntryChangeNotifier
{
    private readonly object _gate = new();
    private readonly List<Action<int>> _handlers = [];

    public void Raise(int centerId)
    {
        Action<int>[] handlers;

        lock (_gate)
        {
            handlers = _handlers.ToArray();
        }

        foreach (var handler in handlers)
        {
            try
            {
                handler(centerId);
            }
            catch (Exception)
            {
            }
        }
    }

    public IDisposable Subscribe(Action<int> handler)
    {
        ArgumentNullException.ThrowIfNull(handler);

        lock (_gate)
        {
            _handlers.Add(handler);
        }

        return new Subscription(this, handler);
    }

    private void Unsubscribe(Action<int> handler)
    {
        lock (_gate)
        {
            _handlers.Remove(handler);
        }
    }

    private sealed class Subscription : IDisposable
    {
        private readonly TimeEntryChangeNotifier _notifier;
        private readonly Action<int> _handler;
        private bool _disposed;

        public Subscription(TimeEntryChangeNotifier notifier, Action<int> handler)
        {
            _notifier = notifier;
            _handler = handler;
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            _notifier.Unsubscribe(_handler);
        }
    }
}
