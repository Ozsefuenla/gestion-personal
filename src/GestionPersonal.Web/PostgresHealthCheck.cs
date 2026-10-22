using Dapper;
using GestionPersonal.Infrastructure.Persistence.Postgres;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace GestionPersonal.Web;

public sealed class PostgresHealthCheck : IHealthCheck
{
    private readonly IDbConnectionFactory _connectionFactory;

    public PostgresHealthCheck(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await using var connection = _connectionFactory.Create();
            await connection.OpenAsync(cancellationToken);

            await connection.ExecuteScalarAsync<int>(
                new CommandDefinition("SELECT 1", cancellationToken: cancellationToken));

            return HealthCheckResult.Healthy("PostgreSQL responde.");
        }
        catch (Exception exception)
        {
            return HealthCheckResult.Unhealthy("PostgreSQL no responde.", exception);
        }
    }
}
