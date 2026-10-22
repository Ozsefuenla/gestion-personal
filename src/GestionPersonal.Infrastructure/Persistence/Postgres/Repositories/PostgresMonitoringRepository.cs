using Dapper;
using GestionPersonal.Application.Abstractions;
using GestionPersonal.Application.Monitoring;

namespace GestionPersonal.Infrastructure.Persistence.Postgres.Repositories;

public sealed class PostgresMonitoringRepository : IMonitoringRepository
{
    private const string OverviewSql =
        """
        SELECT
            (SELECT COUNT(*) FROM centers)::int AS "TotalCenters",
            (SELECT COUNT(*) FROM centers WHERE active)::int AS "ActiveCenters",
            (SELECT COUNT(*) FROM workers WHERE active)::int AS "ActiveWorkers",
            (SELECT COUNT(*) FROM time_entries
                WHERE (started_at AT TIME ZONE 'Europe/Madrid')::date
                    = (now() AT TIME ZONE 'Europe/Madrid')::date)::int AS "ClockInsToday",
            (SELECT COUNT(*) FROM vacations WHERE status = 'Pending')::int AS "PendingAbsences",
            (SELECT COUNT(*) FROM app_exceptions
                WHERE occurred_at >= now() - interval '24 hours')::int AS "ExceptionsLast24h",
            (SELECT COUNT(*) FROM request_metrics
                WHERE occurred_at >= now() - interval '24 hours')::int AS "RequestsLast24h",
            (SELECT COUNT(*) FROM request_metrics
                WHERE occurred_at >= now() - interval '24 hours'
                    AND status_code >= 500)::int AS "ErrorsLast24h"
        """;

    private const string SeveritySql =
        """
        SELECT severity AS "Severity", COUNT(*)::int AS "Count"
        FROM app_events
        WHERE occurred_at >= now() - interval '24 hours'
        GROUP BY severity
        ORDER BY severity
        """;

    private const string ExceptionsSql =
        """
        SELECT
            id AS "Id",
            occurred_at AS "OccurredAt",
            exception_type AS "ExceptionType",
            message AS "Message",
            request_path AS "RequestPath",
            http_method AS "HttpMethod",
            status_code AS "StatusCode",
            actor_role AS "ActorRole",
            center_id AS "CenterId"
        FROM app_exceptions
        WHERE occurred_at >= now() - interval '24 hours'
        ORDER BY occurred_at DESC
        LIMIT @limit OFFSET @offset
        """;

    private const string ErrorSeriesSql =
        """
        SELECT
            date_trunc('hour', occurred_at) AS "Hour",
            COUNT(*)::int AS "Requests",
            COUNT(*) FILTER (WHERE status_code >= 500)::int AS "Errors"
        FROM request_metrics
        WHERE occurred_at >= now() - interval '24 hours'
        GROUP BY 1
        ORDER BY 1
        """;

    private readonly IDbConnectionFactory _connectionFactory;

    public PostgresMonitoringRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<PlatformOverviewResponse> GetOverviewAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = _connectionFactory.Create();

        var row = await connection.QuerySingleAsync<MonitoringOverviewRow>(
            new CommandDefinition(OverviewSql, cancellationToken: cancellationToken));

        var severity = await connection.QueryAsync<SeverityCountRow>(
            new CommandDefinition(SeveritySql, cancellationToken: cancellationToken));

        return new PlatformOverviewResponse(
            row.TotalCenters,
            row.ActiveCenters,
            row.ActiveWorkers,
            row.ClockInsToday,
            row.PendingAbsences,
            row.ExceptionsLast24h,
            row.RequestsLast24h,
            row.ErrorsLast24h,
            severity.Select(item => new SeverityCount(item.Severity, item.Count)).ToList());
    }

    public async Task<IReadOnlyList<ExceptionListItem>> ListExceptionsAsync(
        int limit,
        int offset,
        CancellationToken cancellationToken = default)
    {
        await using var connection = _connectionFactory.Create();

        var rows = await connection.QueryAsync<ExceptionListRow>(
            new CommandDefinition(
                ExceptionsSql,
                new { limit, offset },
                cancellationToken: cancellationToken));

        return rows
            .Select(row => new ExceptionListItem(
                row.Id,
                ToUtc(row.OccurredAt),
                row.ExceptionType,
                row.Message,
                row.RequestPath,
                row.HttpMethod,
                row.StatusCode,
                row.ActorRole,
                row.CenterId))
            .ToList();
    }

    public async Task<IReadOnlyList<ErrorRatePoint>> GetErrorRateSeriesAsync(
        CancellationToken cancellationToken = default)
    {
        await using var connection = _connectionFactory.Create();

        var rows = await connection.QueryAsync<ErrorRateRow>(
            new CommandDefinition(ErrorSeriesSql, cancellationToken: cancellationToken));

        return rows
            .Select(row => new ErrorRatePoint(ToUtc(row.Hour), row.Requests, row.Errors))
            .ToList();
    }

    private static DateTimeOffset ToUtc(DateTime value) =>
        new(DateTime.SpecifyKind(value, DateTimeKind.Utc));
}
