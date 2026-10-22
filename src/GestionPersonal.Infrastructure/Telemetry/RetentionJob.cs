using Dapper;
using GestionPersonal.Infrastructure.Persistence.Postgres;
using Microsoft.Extensions.Hosting;

namespace GestionPersonal.Infrastructure.Telemetry;

public sealed class RetentionJob : BackgroundService
{
    private const string PurgeEventsSql =
        "DELETE FROM app_events WHERE occurred_at < now() - make_interval(days => @days)";

    private const string PurgeExceptionsSql =
        "DELETE FROM app_exceptions WHERE occurred_at < now() - make_interval(days => @days)";

    private const string PurgeRequestMetricsSql =
        "DELETE FROM request_metrics WHERE occurred_at < now() - make_interval(days => @days)";

    private readonly IDbConnectionFactory _connectionFactory;
    private readonly TelemetryOptions _options;

    public RetentionJob(IDbConnectionFactory connectionFactory, TelemetryOptions options)
    {
        _connectionFactory = connectionFactory;
        _options = options;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await PurgeAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Telemetría: fallo en la purga de retención: {ex.Message}");
            }

            try
            {
                await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
            }
            catch (OperationCanceledException)
            {
                return;
            }
        }
    }

    private async Task PurgeAsync(CancellationToken cancellationToken)
    {
        await using var connection = _connectionFactory.Create();
        await connection.OpenAsync(cancellationToken);

        var parameters = new { days = _options.RetentionDays };

        await connection.ExecuteAsync(new CommandDefinition(
            PurgeEventsSql,
            parameters,
            cancellationToken: cancellationToken));

        await connection.ExecuteAsync(new CommandDefinition(
            PurgeExceptionsSql,
            parameters,
            cancellationToken: cancellationToken));

        await connection.ExecuteAsync(new CommandDefinition(
            PurgeRequestMetricsSql,
            parameters,
            cancellationToken: cancellationToken));
    }
}
