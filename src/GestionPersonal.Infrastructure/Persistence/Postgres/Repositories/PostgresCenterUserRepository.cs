using Dapper;
using GestionPersonal.Application.Abstractions;
using GestionPersonal.Domain.Entities;

namespace GestionPersonal.Infrastructure.Persistence.Postgres.Repositories;

public sealed class PostgresCenterUserRepository : ICenterUserRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public PostgresCenterUserRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<CenterUser?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        await using var connection = _connectionFactory.Create();

        var row = await connection.QuerySingleOrDefaultAsync<CenterUserRow>(
            new CommandDefinition(
                "SELECT id, center_id, username, password_hash FROM center_users WHERE username = @username",
                new { username },
                cancellationToken: cancellationToken));

        return row is null
            ? null
            : CenterUser.Create(row.Id, row.CenterId, row.Username, row.PasswordHash);
    }
}
