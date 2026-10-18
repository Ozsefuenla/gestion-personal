using Dapper;
using GestionPersonal.Application.Abstractions;
using GestionPersonal.Domain.Entities;
using GestionPersonal.Domain.Enums;

namespace GestionPersonal.Infrastructure.Persistence.Postgres.Repositories;

public sealed class PostgresCenterUserRepository : ICenterUserRepository
{
    private const string SelectColumns = "id, center_id, username, password_hash, role, active, must_change_password";

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
                $"SELECT {SelectColumns} FROM center_users WHERE username = @username",
                new { username },
                cancellationToken: cancellationToken));

        return row is null ? null : Map(row);
    }

    public async Task<CenterUser?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        await using var connection = _connectionFactory.Create();

        var row = await connection.QuerySingleOrDefaultAsync<CenterUserRow>(
            new CommandDefinition(
                $"SELECT {SelectColumns} FROM center_users WHERE id = @id",
                new { id },
                cancellationToken: cancellationToken));

        return row is null ? null : Map(row);
    }

    public async Task<IReadOnlyList<CenterUser>> GetByCenterAsync(int centerId, CancellationToken cancellationToken = default)
    {
        await using var connection = _connectionFactory.Create();

        var rows = await connection.QueryAsync<CenterUserRow>(
            new CommandDefinition(
                $"SELECT {SelectColumns} FROM center_users WHERE center_id = @centerId ORDER BY username",
                new { centerId },
                cancellationToken: cancellationToken));

        return rows.Select(Map).ToList();
    }

    public async Task<CenterUser> AddAsync(CenterUser user, CancellationToken cancellationToken = default)
    {
        await using var connection = _connectionFactory.Create();

        var id = await connection.ExecuteScalarAsync<int>(
            new CommandDefinition(
                """
                INSERT INTO center_users (center_id, username, password_hash, role, active, must_change_password)
                VALUES (@CenterId, @Username, @PasswordHash, @Role, @Active, @MustChangePassword)
                RETURNING id
                """,
                new
                {
                    user.CenterId,
                    user.Username,
                    user.PasswordHash,
                    Role = user.Role.ToString(),
                    user.Active,
                    user.MustChangePassword
                },
                cancellationToken: cancellationToken));

        return CenterUser.Rehydrate(
            id,
            user.CenterId,
            user.Username,
            user.PasswordHash,
            user.Role,
            user.Active,
            user.MustChangePassword);
    }

    public async Task UpdateAsync(CenterUser user, CancellationToken cancellationToken = default)
    {
        await using var connection = _connectionFactory.Create();

        await connection.ExecuteAsync(
            new CommandDefinition(
                """
                UPDATE center_users
                SET center_id = @CenterId,
                    username = @Username,
                    password_hash = @PasswordHash,
                    role = @Role,
                    active = @Active,
                    must_change_password = @MustChangePassword
                WHERE id = @Id
                """,
                new
                {
                    user.Id,
                    user.CenterId,
                    user.Username,
                    user.PasswordHash,
                    Role = user.Role.ToString(),
                    user.Active,
                    user.MustChangePassword
                },
                cancellationToken: cancellationToken));
    }

    private static CenterUser Map(CenterUserRow row) =>
        CenterUser.Rehydrate(
            row.Id,
            row.CenterId,
            row.Username,
            row.PasswordHash,
            Enum.Parse<UserRole>(row.Role),
            row.Active,
            row.MustChangePassword);
}
