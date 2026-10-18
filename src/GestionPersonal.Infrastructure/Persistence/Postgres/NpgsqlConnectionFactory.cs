using Dapper;
using Npgsql;

namespace GestionPersonal.Infrastructure.Persistence.Postgres;

public sealed class NpgsqlConnectionFactory : IDbConnectionFactory
{
    private readonly string _connectionString;

    public NpgsqlConnectionFactory(string connectionString)
    {
        _connectionString = connectionString;
        DefaultTypeMap.MatchNamesWithUnderscores = true;
    }

    public NpgsqlConnection Create() => new(_connectionString);
}
