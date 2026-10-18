using Dapper;
using DbUp;
using GestionPersonal.Application.Abstractions;
using GestionPersonal.Domain.Enums;

namespace GestionPersonal.Infrastructure.Persistence.Postgres;

public sealed class DatabaseInitializer
{
    private const int DemoCenterId = 1;

    private static readonly (string FullName, string Pin)[] DemoWorkers =
    [
        ("María García", "1111"),
        ("Ana López", "2222")
    ];

    private readonly IDbConnectionFactory _connectionFactory;
    private readonly string _connectionString;
    private readonly IPasswordHasher _passwordHasher;
    private readonly bool _seedDemoData;
    private readonly SeedCredentials _credentials;

    public DatabaseInitializer(
        IDbConnectionFactory connectionFactory,
        string connectionString,
        IPasswordHasher passwordHasher,
        bool seedDemoData,
        SeedCredentials credentials)
    {
        _connectionFactory = connectionFactory;
        _connectionString = connectionString;
        _passwordHasher = passwordHasher;
        _seedDemoData = seedDemoData;
        _credentials = credentials;
    }

    public void Initialize()
    {
        RunMigrations();
        SeedAsync().GetAwaiter().GetResult();
    }

    private void RunMigrations()
    {
        var upgrader = DeployChanges.To
            .PostgresqlDatabase(_connectionString)
            .WithScriptsEmbeddedInAssembly(typeof(DatabaseInitializer).Assembly)
            .LogToConsole()
            .Build();

        var result = upgrader.PerformUpgrade();

        if (!result.Successful)
        {
            throw new InvalidOperationException(
                $"No se pudieron aplicar las migraciones de base de datos: {result.Error?.Message}",
                result.Error);
        }
    }

    private async Task SeedAsync()
    {
        await using var connection = _connectionFactory.Create();
        await connection.OpenAsync();
        await using var transaction = await connection.BeginTransactionAsync();

        await EnsureCenterAsync(connection, transaction);
        await EnsureSuperAdminAsync(connection, transaction);
        await EnsureCenterAdminAsync(connection, transaction);
        await EnsureCenterKioskAsync(connection, transaction);

        if (_seedDemoData)
        {
            await EnsureDemoDataAsync(connection, transaction);
        }

        await transaction.CommitAsync();
    }

    private static async Task EnsureCenterAsync(
        Npgsql.NpgsqlConnection connection,
        System.Data.Common.DbTransaction transaction)
    {
        await connection.ExecuteAsync(
            new CommandDefinition(
                """
                INSERT INTO centers (id, name, active)
                VALUES (@id, @name, TRUE)
                ON CONFLICT (id) DO NOTHING
                """,
                new { id = DemoCenterId, name = "Peluquería Sol" },
                transaction));

        await connection.ExecuteAsync(
            new CommandDefinition(
                "SELECT setval(pg_get_serial_sequence('centers', 'id'), GREATEST((SELECT COALESCE(MAX(id), 0) FROM centers), 1))",
                transaction: transaction));
    }

    private async Task EnsureSuperAdminAsync(
        Npgsql.NpgsqlConnection connection,
        System.Data.Common.DbTransaction transaction)
    {
        await InsertUserAsync(
            connection,
            transaction,
            centerId: null,
            _credentials.SuperAdminUsername,
            _credentials.SuperAdminPassword,
            UserRole.SuperAdmin);
    }

    private async Task EnsureCenterAdminAsync(
        Npgsql.NpgsqlConnection connection,
        System.Data.Common.DbTransaction transaction)
    {
        await InsertUserAsync(
            connection,
            transaction,
            DemoCenterId,
            _credentials.CenterAdminUsername,
            _credentials.CenterAdminPassword,
            UserRole.CenterAdmin);
    }

    private async Task EnsureCenterKioskAsync(
        Npgsql.NpgsqlConnection connection,
        System.Data.Common.DbTransaction transaction)
    {
        await InsertUserAsync(
            connection,
            transaction,
            DemoCenterId,
            _credentials.CenterUsername,
            _credentials.CenterPassword,
            UserRole.Center);
    }

    private async Task InsertUserAsync(
        Npgsql.NpgsqlConnection connection,
        System.Data.Common.DbTransaction transaction,
        int? centerId,
        string username,
        string password,
        UserRole role)
    {
        await connection.ExecuteAsync(
            new CommandDefinition(
                """
                INSERT INTO center_users (center_id, username, password_hash, role, active)
                VALUES (@centerId, @username, @passwordHash, @role, TRUE)
                ON CONFLICT (username) DO NOTHING
                """,
                new
                {
                    centerId,
                    username,
                    passwordHash = _passwordHasher.Hash(password),
                    role = role.ToString()
                },
                transaction));
    }

    private async Task EnsureDemoDataAsync(
        Npgsql.NpgsqlConnection connection,
        System.Data.Common.DbTransaction transaction)
    {
        var workerIds = new List<int>(DemoWorkers.Length);

        foreach (var (fullName, pin) in DemoWorkers)
        {
            var existingId = await connection.ExecuteScalarAsync<int?>(
                new CommandDefinition(
                    "SELECT id FROM workers WHERE center_id = @centerId AND full_name = @fullName LIMIT 1",
                    new { centerId = DemoCenterId, fullName },
                    transaction));

            if (existingId is not null)
            {
                workerIds.Add(existingId.Value);
                continue;
            }

            var id = await connection.ExecuteScalarAsync<int>(
                new CommandDefinition(
                    """
                    INSERT INTO workers (center_id, full_name, pin_hash)
                    VALUES (@centerId, @fullName, @pinHash)
                    RETURNING id
                    """,
                    new
                    {
                        centerId = DemoCenterId,
                        fullName,
                        pinHash = _passwordHasher.Hash(pin)
                    },
                    transaction));

            workerIds.Add(id);
        }

        var entryCount = await connection.ExecuteScalarAsync<int>(
            new CommandDefinition("SELECT COUNT(*) FROM time_entries", transaction: transaction));

        if (entryCount > 0)
        {
            return;
        }

        foreach (var entry in SeedData.BuildTimeEntries(workerIds))
        {
            await connection.ExecuteAsync(
                new CommandDefinition(
                    """
                    INSERT INTO time_entries (id, worker_id, status, started_at, ended_at)
                    VALUES (@Id, @WorkerId, @Status, @StartedAt, @EndedAt)
                    """,
                    new
                    {
                        entry.Id,
                        entry.WorkerId,
                        Status = entry.Status.ToString(),
                        StartedAt = PgConvert.ToUtc(entry.StartedAt),
                        EndedAt = PgConvert.ToUtc(entry.EndedAt)
                    },
                    transaction));

            foreach (var pause in entry.Pauses)
            {
                await connection.ExecuteAsync(
                    new CommandDefinition(
                        """
                        INSERT INTO pause_periods (time_entry_id, paused_at, resumed_at)
                        VALUES (@timeEntryId, @pausedAt, @resumedAt)
                        """,
                        new
                        {
                            timeEntryId = entry.Id,
                            pausedAt = PgConvert.ToUtc(pause.PausedAt),
                            resumedAt = PgConvert.ToUtc(pause.ResumedAt)
                        },
                        transaction));
            }
        }
    }
}
