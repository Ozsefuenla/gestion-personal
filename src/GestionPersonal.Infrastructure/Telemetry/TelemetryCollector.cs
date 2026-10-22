using System.Text.Json;
using System.Threading.Channels;
using GestionPersonal.Application.Abstractions;

namespace GestionPersonal.Infrastructure.Telemetry;

public sealed class TelemetryCollector : IEventCollector, IExceptionReporter, IRequestMetricsRecorder
{
    private readonly Channel<TelemetryRecord> _channel;
    private long _dropped;

    public TelemetryCollector(TelemetryOptions options)
    {
        _channel = Channel.CreateBounded<TelemetryRecord>(new BoundedChannelOptions(options.ChannelCapacity)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = true,
            SingleWriter = false
        });
    }

    internal ChannelReader<TelemetryRecord> Reader => _channel.Reader;

    public long DroppedCount => Interlocked.Read(ref _dropped);

    public void Emit(AppEvent appEvent)
    {
        var record = new TelemetryEventRecord(
            DateTimeOffset.UtcNow,
            appEvent.Category,
            appEvent.EventType,
            appEvent.Severity,
            appEvent.ActorRole?.ToString(),
            appEvent.ActorId,
            appEvent.CenterId,
            appEvent.Message,
            Serialize(appEvent.Data),
            appEvent.CorrelationId);

        TryWrite(record);
    }

    public void Report(ReportedException reportedException)
    {
        var record = new TelemetryExceptionRecord(
            DateTimeOffset.UtcNow,
            reportedException.ExceptionType,
            reportedException.Message,
            reportedException.StackTrace,
            reportedException.RequestPath,
            reportedException.HttpMethod,
            reportedException.StatusCode,
            reportedException.ActorRole?.ToString(),
            reportedException.ActorId,
            reportedException.CenterId,
            reportedException.CorrelationId);

        TryWrite(record);
    }

    public void Record(RequestMetric requestMetric)
    {
        var record = new RequestMetricRecord(
            DateTimeOffset.UtcNow,
            requestMetric.Path,
            requestMetric.HttpMethod,
            requestMetric.StatusCode,
            requestMetric.DurationMs,
            requestMetric.CenterId);

        TryWrite(record);
    }

    private void TryWrite(TelemetryRecord record)
    {
        if (!_channel.Writer.TryWrite(record))
        {
            Interlocked.Increment(ref _dropped);
        }
    }

    private static string? Serialize(IReadOnlyDictionary<string, object?>? data) =>
        data is null || data.Count == 0 ? null : JsonSerializer.Serialize(data);
}
