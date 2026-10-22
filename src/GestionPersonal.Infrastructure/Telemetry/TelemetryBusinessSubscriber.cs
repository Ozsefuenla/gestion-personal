using GestionPersonal.Application.Abstractions;
using Microsoft.Extensions.Hosting;

namespace GestionPersonal.Infrastructure.Telemetry;

public sealed class TelemetryBusinessSubscriber : IHostedService, IDisposable
{
    private readonly ITimeEntryChangeNotifier _notifier;
    private readonly IEventCollector _events;
    private IDisposable? _subscription;

    public TelemetryBusinessSubscriber(ITimeEntryChangeNotifier notifier, IEventCollector events)
    {
        _notifier = notifier;
        _events = events;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _subscription = _notifier.Subscribe(Raise);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _subscription?.Dispose();
        _subscription = null;
        return Task.CompletedTask;
    }

    public void Dispose() => _subscription?.Dispose();

    private void Raise(int centerId) =>
        _events.Emit(new AppEvent(
            "business",
            "time_entry.changed",
            "Info",
            "Fichaje actualizado",
            CenterId: centerId));
}
