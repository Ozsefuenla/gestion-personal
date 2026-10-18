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
                "SELECT id, name FROM centers WHERE id = @id",
                new { id },
                cancellationToken: cancellationToken));

        return row is null ? null : Center.Create(row.Id, row.Name);
    }

    public async Task<IReadOnlyList<Center>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = _connectionFactory.Create();

        var rows = await connection.QueryAsync<CenterRow>(
            new CommandDefinition(
                "SELECT id, name FROM centers ORDER BY id",
                cancellationToken: cancellationToken));

        return rows.Select(row => Center.Create(row.Id, row.Name)).ToList();
    }
}
