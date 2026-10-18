using Npgsql;

namespace GestionPersonal.Infrastructure.Persistence.Postgres;

public interface IDbConnectionFactory
{
    NpgsqlConnection Create();
}
