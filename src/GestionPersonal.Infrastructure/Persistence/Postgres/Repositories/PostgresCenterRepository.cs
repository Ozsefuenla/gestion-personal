using Dapper;
using GestionPersonal.Application.Abstractions;
using GestionPersonal.Domain.Entities;

namespace GestionPersonal.Infrastructure.Persistence.Postgres.Repositories;

public sealed class PostgresCenterRepository : ICenterRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public PostgresCenterRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<Center?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        await using var connection = _connectionFactory.Create();

        var row = await connection.QuerySingleOrDefaultAsync<CenterRow>(
            new CommandDefinition(
                "SELECT id, name, active FROM centers WHERE id = @id",
                new { id },
                cancellationToken: cancellationToken));

        return row is null ? null : Map(row);
    }

    public async Task<IReadOnlyList<Center>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = _connectionFactory.Create();

        var rows = await connection.QueryAsync<CenterRow>(
            new CommandDefinition(
                "SELECT id, name, active FROM centers ORDER BY id",
                cancellationToken: cancellationToken));

        return rows.Select(Map).ToList();
    }

    public async Task<Center> AddAsync(Center center, CancellationToken cancellationToken = default)
    {
        await using var connection = _connectionFactory.Create();

        var id = await connection.ExecuteScalarAsync<int>(
            new CommandDefinition(
                """
                INSERT INTO centers (name, active)
                VALUES (@Name, @Active)
                RETURNING id
                """,
                new { center.Name, center.Active },
                cancellationToken: cancellationToken));

        return Center.Rehydrate(id, center.Name, center.Active);
    }

    public async Task UpdateAsync(Center center, CancellationToken cancellationToken = default)
    {
        await using var connection = _connectionFactory.Create();

        await connection.ExecuteAsync(
            new CommandDefinition(
                """
                UPDATE centers
                SET name = @Name,
                    active = @Active
                WHERE id = @Id
                """,
                new { center.Id, center.Name, center.Active },
                cancellationToken: cancellationToken));
    }

    private static Center Map(CenterRow row) =>
        Center.Rehydrate(row.Id, row.Name, row.Active);
}
