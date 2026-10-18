using Dapper;
using GestionPersonal.Application.Abstractions;
using GestionPersonal.Domain.Entities;
using GestionPersonal.Domain.Enums;
using Npgsql;

namespace GestionPersonal.Infrastructure.Persistence.Postgres.Repositories;

public sealed class PostgresTimeEntryRepository : ITimeEntryRepository
{
    private const string SelectColumns = "id, worker_id, status, started_at, ended_at";

    private readonly IDbConnectionFactory _connectionFactory;

    public PostgresTimeEntryRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<TimeEntry?> GetOpenEntryByWorkerAsync(int workerId, CancellationToken cancellationToken = default)
    {
        await using var connection = _connectionFactory.Create();

        var row = await connection.QuerySingleOrDefaultAsync<TimeEntryRow>(
            new CommandDefinition(
                $"""
                SELECT {SelectColumns} FROM time_entries
                WHERE worker_id = @workerId AND status IN ('InProgress', 'Paused')
                ORDER BY started_at DESC
                LIMIT 1
                """,
                new { workerId },
                cancellationToken: cancellationToken));

        if (row is null)
        {
            return null;
        }

        var pauses = await LoadPausesAsync(connection, [row.Id], cancellationToken);

        return Map(row, pauses.GetValueOrDefault(row.Id, []));
    }

    public async Task<IReadOnlyList<TimeEntry>> GetByWorkerBetweenAsync(
        int workerId,
        DateTimeOffset from,
        DateTimeOffset to,
        CancellationToken cancellationToken = default)
    {
        await using var connection = _connectionFactory.Create();

        var rows = await connection.QueryAsync<TimeEntryRow>(
            new CommandDefinition(
                $"""
                SELECT {SelectColumns} FROM time_entries
                WHERE worker_id = @workerId AND started_at >= @from AND started_at < @to
                ORDER BY started_at
                """,
                new { workerId, from = PgConvert.ToUtc(from), to = PgConvert.ToUtc(to) },
                cancellationToken: cancellationToken));

        return await MapManyAsync(connection, rows, cancellationToken);
    }

    public async Task<IReadOnlyList<TimeEntry>> GetByWorkerOverlappingAsync(
        int workerId,
        DateTimeOffset from,
        DateTimeOffset to,
        CancellationToken cancellationToken = default)
    {
        await using var connection = _connectionFactory.Create();

        var rows = await connection.QueryAsync<TimeEntryRow>(
            new CommandDefinition(
                $"""
                SELECT {SelectColumns} FROM time_entries
                WHERE worker_id = @workerId
                  AND started_at < @to
                  AND (ended_at IS NULL OR ended_at >= @from)
                ORDER BY started_at
                """,
                new { workerId, from = PgConvert.ToUtc(from), to = PgConvert.ToUtc(to) },
                cancellationToken: cancellationToken));

        return await MapManyAsync(connection, rows, cancellationToken);
    }

    public async Task AddAsync(TimeEntry timeEntry, CancellationToken cancellationToken = default)
    {
        await using var connection = _connectionFactory.Create();
        await connection.OpenAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        await connection.ExecuteAsync(
            new CommandDefinition(
                """
                INSERT INTO time_entries (id, worker_id, status, started_at, ended_at)
                VALUES (@Id, @WorkerId, @Status, @StartedAt, @EndedAt)
                """,
                new
                {
                    timeEntry.Id,
                    timeEntry.WorkerId,
                    Status = timeEntry.Status.ToString(),
                    StartedAt = PgConvert.ToUtc(timeEntry.StartedAt),
                    EndedAt = PgConvert.ToUtc(timeEntry.EndedAt)
                },
                transaction,
                cancellationToken: cancellationToken));

        await InsertPausesAsync(connection, transaction, timeEntry, cancellationToken);

        await transaction.CommitAsync(cancellationToken);
    }

    public async Task UpdateAsync(TimeEntry timeEntry, CancellationToken cancellationToken = default)
    {
        await using var connection = _connectionFactory.Create();
        await connection.OpenAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        await connection.ExecuteAsync(
            new CommandDefinition(
                """
                UPDATE time_entries
                SET status = @Status,
                    started_at = @StartedAt,
                    ended_at = @EndedAt
                WHERE id = @Id
                """,
                new
                {
                    timeEntry.Id,
                    Status = timeEntry.Status.ToString(),
                    StartedAt = PgConvert.ToUtc(timeEntry.StartedAt),
                    EndedAt = PgConvert.ToUtc(timeEntry.EndedAt)
                },
                transaction,
                cancellationToken: cancellationToken));

        await connection.ExecuteAsync(
            new CommandDefinition(
                "DELETE FROM pause_periods WHERE time_entry_id = @Id",
                new { timeEntry.Id },
                transaction,
                cancellationToken: cancellationToken));

        await InsertPausesAsync(connection, transaction, timeEntry, cancellationToken);

        await transaction.CommitAsync(cancellationToken);
    }

    public async Task RemoveAsync(TimeEntry timeEntry, CancellationToken cancellationToken = default)
    {
        await using var connection = _connectionFactory.Create();

        await connection.ExecuteAsync(
            new CommandDefinition(
                "DELETE FROM time_entries WHERE id = @Id",
                new { timeEntry.Id },
                cancellationToken: cancellationToken));
    }

    private static async Task<IReadOnlyList<TimeEntry>> MapManyAsync(
        NpgsqlConnection connection,
        IEnumerable<TimeEntryRow> rows,
        CancellationToken cancellationToken)
    {
        var materialized = rows.ToList();

        if (materialized.Count == 0)
        {
            return [];
        }

        var pauses = await LoadPausesAsync(connection, materialized.Select(row => row.Id).ToList(), cancellationToken);

        return materialized
            .Select(row => Map(row, pauses.GetValueOrDefault(row.Id, [])))
            .ToList();
    }

    private static async Task<Dictionary<Guid, List<PausePeriod>>> LoadPausesAsync(
        NpgsqlConnection connection,
        IReadOnlyList<Guid> entryIds,
        CancellationToken cancellationToken)
    {
        var rows = await connection.QueryAsync<PauseRow>(
            new CommandDefinition(
                """
                SELECT time_entry_id, paused_at, resumed_at
                FROM pause_periods
                WHERE time_entry_id = ANY(@entryIds)
                ORDER BY paused_at
                """,
                new { entryIds = entryIds.ToArray() },
                cancellationToken: cancellationToken));

        return rows
            .GroupBy(row => row.TimeEntryId)
            .ToDictionary(
                group => group.Key,
                group => group
                    .Select(row => PausePeriod.Rehydrate(
                        PgConvert.ToOffset(row.PausedAt),
                        PgConvert.ToOffset(row.ResumedAt)))
                    .ToList());
    }

    private static async Task InsertPausesAsync(
        NpgsqlConnection connection,
        System.Data.Common.DbTransaction transaction,
        TimeEntry timeEntry,
        CancellationToken cancellationToken)
    {
        if (timeEntry.Pauses.Count == 0)
        {
            return;
        }

        await connection.ExecuteAsync(
            new CommandDefinition(
                """
                INSERT INTO pause_periods (time_entry_id, paused_at, resumed_at)
                VALUES (@TimeEntryId, @PausedAt, @ResumedAt)
                """,
                timeEntry.Pauses.Select(pause => new
                {
                    TimeEntryId = timeEntry.Id,
                    PausedAt = PgConvert.ToUtc(pause.PausedAt),
                    ResumedAt = PgConvert.ToUtc(pause.ResumedAt)
                }),
                transaction,
                cancellationToken: cancellationToken));
    }

    private static TimeEntry Map(TimeEntryRow row, IReadOnlyList<PausePeriod> pauses) =>
        TimeEntry.Rehydrate(
            row.Id,
            row.WorkerId,
            Enum.Parse<TimeEntryStatus>(row.Status, ignoreCase: true),
            PgConvert.ToOffset(row.StartedAt),
            PgConvert.ToOffset(row.EndedAt),
            pauses);
}
