using Dapper;
using GestionPersonal.Application.Abstractions;
using GestionPersonal.Domain.Entities;
using GestionPersonal.Domain.Enums;

namespace GestionPersonal.Infrastructure.Persistence.Postgres.Repositories;

public sealed class PostgresVacationRepository : IVacationRepository
{
    private const string SelectColumns =
        "id, worker_id, start_date, end_date, type, status, requested_at, decided_at";

    private readonly IDbConnectionFactory _connectionFactory;

    public PostgresVacationRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IReadOnlyList<Vacation>> GetByWorkerAsync(int workerId, CancellationToken cancellationToken = default)
    {
        await using var connection = _connectionFactory.Create();

        var rows = await connection.QueryAsync<VacationRow>(
            new CommandDefinition(
                $"""
                SELECT {SelectColumns} FROM vacations
                WHERE worker_id = @workerId AND status <> 'Deleted'
                ORDER BY start_date DESC
                """,
                new { workerId },
                cancellationToken: cancellationToken));

        return rows.Select(Map).ToList();
    }

    public async Task<Vacation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await using var connection = _connectionFactory.Create();

        var row = await connection.QuerySingleOrDefaultAsync<VacationRow>(
            new CommandDefinition(
                $"SELECT {SelectColumns} FROM vacations WHERE id = @id",
                new { id },
                cancellationToken: cancellationToken));

        return row is null ? null : Map(row);
    }

    public async Task<IReadOnlyList<Vacation>> GetApprovedByWorkerBetweenAsync(
        int workerId,
        DateOnly from,
        DateOnly to,
        CancellationToken cancellationToken = default)
    {
        await using var connection = _connectionFactory.Create();

        var rows = await connection.QueryAsync<VacationRow>(
            new CommandDefinition(
                $"""
                SELECT {SelectColumns} FROM vacations
                WHERE worker_id = @workerId
                  AND status = 'Approved'
                  AND start_date <= @to
                  AND end_date >= @from
                ORDER BY start_date
                """,
                new { workerId, from, to },
                cancellationToken: cancellationToken));

        return rows.Select(Map).ToList();
    }

    public async Task<bool> HasOverlappingActiveAsync(
        int workerId,
        DateOnly startDate,
        DateOnly endDate,
        CancellationToken cancellationToken = default)
    {
        await using var connection = _connectionFactory.Create();

        return await connection.ExecuteScalarAsync<bool>(
            new CommandDefinition(
                """
                SELECT EXISTS (
                    SELECT 1 FROM vacations
                    WHERE worker_id = @workerId
                      AND status <> 'Deleted'
                      AND start_date <= @endDate
                      AND end_date >= @startDate
                )
                """,
                new { workerId, startDate, endDate },
                cancellationToken: cancellationToken));
    }

    public async Task<IReadOnlyList<Vacation>> GetByWorkerIdsAsync(
        IReadOnlyList<int> workerIds,
        CancellationToken cancellationToken = default)
    {
        if (workerIds.Count == 0)
        {
            return [];
        }

        await using var connection = _connectionFactory.Create();

        var rows = await connection.QueryAsync<VacationRow>(
            new CommandDefinition(
                $"""
                SELECT {SelectColumns} FROM vacations
                WHERE worker_id = ANY(@workerIds) AND status <> 'Deleted'
                ORDER BY start_date DESC
                """,
                new { workerIds = workerIds.ToArray() },
                cancellationToken: cancellationToken));

        return rows.Select(Map).ToList();
    }

    public async Task AddAsync(Vacation vacation, CancellationToken cancellationToken = default)
    {
        await using var connection = _connectionFactory.Create();

        await connection.ExecuteAsync(
            new CommandDefinition(
                """
                INSERT INTO vacations (id, worker_id, start_date, end_date, type, status, requested_at, decided_at)
                VALUES (@Id, @WorkerId, @StartDate, @EndDate, @Type, @Status, @RequestedAt, @DecidedAt)
                """,
                new
                {
                    vacation.Id,
                    vacation.WorkerId,
                    vacation.StartDate,
                    vacation.EndDate,
                    Type = vacation.Type.ToString(),
                    Status = vacation.Status.ToString(),
                    RequestedAt = PgConvert.ToUtc(vacation.RequestedAt),
                    DecidedAt = PgConvert.ToUtc(vacation.DecidedAt)
                },
                cancellationToken: cancellationToken));
    }

    public async Task UpdateAsync(Vacation vacation, CancellationToken cancellationToken = default)
    {
        await using var connection = _connectionFactory.Create();

        await connection.ExecuteAsync(
            new CommandDefinition(
                """
                UPDATE vacations
                SET start_date = @StartDate,
                    end_date = @EndDate,
                    type = @Type,
                    status = @Status,
                    decided_at = @DecidedAt
                WHERE id = @Id
                """,
                new
                {
                    vacation.Id,
                    vacation.StartDate,
                    vacation.EndDate,
                    Type = vacation.Type.ToString(),
                    Status = vacation.Status.ToString(),
                    DecidedAt = PgConvert.ToUtc(vacation.DecidedAt)
                },
                cancellationToken: cancellationToken));
    }

    private static Vacation Map(VacationRow row) =>
        Vacation.Rehydrate(
            row.Id,
            row.WorkerId,
            row.StartDate,
            row.EndDate,
            Enum.Parse<VacationType>(row.Type, ignoreCase: true),
            Enum.Parse<VacationStatus>(row.Status, ignoreCase: true),
            PgConvert.ToOffset(row.RequestedAt),
            PgConvert.ToOffset(row.DecidedAt));
}
