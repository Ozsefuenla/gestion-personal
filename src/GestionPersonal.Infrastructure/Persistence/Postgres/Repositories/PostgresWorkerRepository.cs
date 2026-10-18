using Dapper;
using GestionPersonal.Application.Abstractions;
using GestionPersonal.Domain.Entities;
using GestionPersonal.Domain.Enums;

namespace GestionPersonal.Infrastructure.Persistence.Postgres.Repositories;

public sealed class PostgresWorkerRepository : IWorkerRepository
{
    private const string SelectColumns =
        "id, center_id, full_name, pin_hash, role, active, must_change_pin, daily_hours, created_at";

    private readonly IDbConnectionFactory _connectionFactory;

    public PostgresWorkerRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<Worker?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        await using var connection = _connectionFactory.Create();

        var row = await connection.QuerySingleOrDefaultAsync<WorkerRow>(
            new CommandDefinition(
                $"SELECT {SelectColumns} FROM workers WHERE id = @id",
                new { id },
                cancellationToken: cancellationToken));

        return row is null ? null : Map(row);
    }

    public async Task<Worker?> GetByIdAndCenterAsync(int id, int centerId, CancellationToken cancellationToken = default)
    {
        await using var connection = _connectionFactory.Create();

        var row = await connection.QuerySingleOrDefaultAsync<WorkerRow>(
            new CommandDefinition(
                $"SELECT {SelectColumns} FROM workers WHERE id = @id AND center_id = @centerId",
                new { id, centerId },
                cancellationToken: cancellationToken));

        return row is null ? null : Map(row);
    }

    public async Task<IReadOnlyList<Worker>> GetByCenterAsync(int centerId, CancellationToken cancellationToken = default)
    {
        await using var connection = _connectionFactory.Create();

        var rows = await connection.QueryAsync<WorkerRow>(
            new CommandDefinition(
                $"SELECT {SelectColumns} FROM workers WHERE center_id = @centerId ORDER BY id",
                new { centerId },
                cancellationToken: cancellationToken));

        return rows.Select(Map).ToList();
    }

    public async Task<IReadOnlyList<Worker>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = _connectionFactory.Create();

        var rows = await connection.QueryAsync<WorkerRow>(
            new CommandDefinition(
                $"SELECT {SelectColumns} FROM workers ORDER BY id",
                cancellationToken: cancellationToken));

        return rows.Select(Map).ToList();
    }

    public async Task<Worker> AddAsync(Worker worker, CancellationToken cancellationToken = default)
    {
        await using var connection = _connectionFactory.Create();

        var id = await connection.ExecuteScalarAsync<int>(
            new CommandDefinition(
                """
                INSERT INTO workers (center_id, full_name, pin_hash, role, active, must_change_pin, daily_hours, created_at)
                VALUES (@CenterId, @FullName, @PinHash, @Role, @Active, @MustChangePin, @DailyHours, @CreatedAt)
                RETURNING id
                """,
                new
                {
                    worker.CenterId,
                    worker.FullName,
                    worker.PinHash,
                    Role = worker.Role.ToString(),
                    worker.Active,
                    worker.MustChangePin,
                    worker.DailyHours,
                    CreatedAt = PgConvert.ToUtc(worker.CreatedAt)
                },
                cancellationToken: cancellationToken));

        return Worker.Rehydrate(
            id,
            worker.CenterId,
            worker.FullName,
            worker.PinHash,
            worker.Role,
            worker.Active,
            worker.MustChangePin,
            worker.DailyHours,
            worker.CreatedAt);
    }

    public async Task UpdateAsync(Worker worker, CancellationToken cancellationToken = default)
    {
        await using var connection = _connectionFactory.Create();

        await connection.ExecuteAsync(
            new CommandDefinition(
                """
                UPDATE workers
                SET full_name = @FullName,
                    pin_hash = @PinHash,
                    role = @Role,
                    active = @Active,
                    must_change_pin = @MustChangePin,
                    daily_hours = @DailyHours
                WHERE id = @Id
                """,
                new
                {
                    worker.Id,
                    worker.FullName,
                    worker.PinHash,
                    Role = worker.Role.ToString(),
                    worker.Active,
                    worker.MustChangePin,
                    worker.DailyHours
                },
                cancellationToken: cancellationToken));
    }

    private static Worker Map(WorkerRow row) =>
        Worker.Rehydrate(
            row.Id,
            row.CenterId,
            row.FullName,
            row.PinHash,
            Enum.Parse<Role>(row.Role, ignoreCase: true),
            row.Active,
            row.MustChangePin,
            row.DailyHours,
            PgConvert.ToOffset(row.CreatedAt));
}
