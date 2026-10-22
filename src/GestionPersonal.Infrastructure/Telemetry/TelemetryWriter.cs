using Dapper;
using GestionPersonal.Infrastructure.Persistence.Postgres;
using Microsoft.Extensions.Hosting;

namespace GestionPersonal.Infrastructure.Telemetry;

public sealed class TelemetryWriter : BackgroundService
{
    private const string InsertEventSql =
        """
        INSERT INTO app_events
            (occurred_at, category, event_type, severity, actor_role, actor_id, center_id, message, data, correlation_id)
        VALUES
            (@OccurredAt, @Category, @EventType, @Severity, @ActorRole, @ActorId, @CenterId, @Message, CAST(@DataJson AS jsonb), CAST(@CorrelationId AS uuid))
        """;

    private const string InsertExceptionSql =
        """
        INSERT INTO app_exceptions
            (occurred_at, exception_type, message, stack_trace, request_path, http_method, status_code, actor_role, actor_id, center_id, correlation_id)
        VALUES
            (@OccurredAt, @ExceptionType, @Message, @StackTrace, @RequestPath, @HttpMethod, @StatusCode, @ActorRole, @ActorId, @CenterId, CAST(@CorrelationId AS uuid))
        """;

    private const string InsertRequestMetricSql =
        """
        INSERT INTO request_metrics
            (occurred_at, path, http_method, status_code, duration_ms, center_id)
        VALUES
            (@OccurredAt, @Path, @HttpMethod, @StatusCode, @DurationMs, @CenterId)
        """;

    private readonly TelemetryCollector _collector;
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly TelemetryOptions _options;

    public TelemetryWriter(
        TelemetryCollector collector,
        IDbConnectionFactory connectionFactory,
        TelemetryOptions options)
    {
        _collector = collector;
        _connectionFactory = connectionFactory;
        _options = options;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var batch = new List<TelemetryRecord>(_options.BatchSize);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                if (!await _collector.Reader.WaitToReadAsync(stoppingToken))
                {
                    return;
                }

                batch.Clear();

                while (batch.Count < _options.BatchSize && _collector.Reader.TryRead(out var record))
                {
                    batch.Add(record);
                }

                if (batch.Count > 0)
                {
                    await PersistAsync(batch, stoppingToken);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Telemetría: no se pudieron persistir los registros: {ex.Message}");

                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    return;
                }
            }
        }
    }

    private async Task PersistAsync(IReadOnlyList<TelemetryRecord> batch, CancellationToken cancellationToken)
    {
        await using var connection = _connectionFactory.Create();
        await connection.OpenAsync(cancellationToken);

        foreach (var record in batch)
        {
            switch (record)
            {
                case TelemetryEventRecord telemetryEvent:
                    await connection.ExecuteAsync(new CommandDefinition(
                        InsertEventSql,
                        new
                        {
                            telemetryEvent.OccurredAt,
                            telemetryEvent.Category,
                            telemetryEvent.EventType,
                            telemetryEvent.Severity,
                            telemetryEvent.ActorRole,
                            telemetryEvent.ActorId,
                            telemetryEvent.CenterId,
                            telemetryEvent.Message,
                            telemetryEvent.DataJson,
                            telemetryEvent.CorrelationId
                        },
                        cancellationToken: cancellationToken));
                    break;

                case TelemetryExceptionRecord telemetryException:
                    await connection.ExecuteAsync(new CommandDefinition(
                        InsertExceptionSql,
                        new
                        {
                            telemetryException.OccurredAt,
                            telemetryException.ExceptionType,
                            telemetryException.Message,
                            telemetryException.StackTrace,
                            telemetryException.RequestPath,
                            telemetryException.HttpMethod,
                            telemetryException.StatusCode,
                            telemetryException.ActorRole,
                            telemetryException.ActorId,
                            telemetryException.CenterId,
                            telemetryException.CorrelationId
                        },
                        cancellationToken: cancellationToken));
                    break;

                case RequestMetricRecord requestMetric:
                    await connection.ExecuteAsync(new CommandDefinition(
                        InsertRequestMetricSql,
                        new
                        {
                            requestMetric.OccurredAt,
                            requestMetric.Path,
                            requestMetric.HttpMethod,
                            requestMetric.StatusCode,
                            requestMetric.DurationMs,
                            requestMetric.CenterId
                        },
                        cancellationToken: cancellationToken));
                    break;
            }
        }
    }
}
