using Dapper;
using DbUp;
using GestionPersonal.Application.Abstractions;
using GestionPersonal.Domain.Enums;

namespace GestionPersonal.Infrastructure.Persistence.Postgres;

public sealed class DatabaseInitializer
{
    private const int DemoCenterId = 1;

    private static readonly (string FullName, string Pin, Role Role)[] DemoWorkers =
    [
        ("María García", "1111", Role.Worker),
        ("Ana López", "2222", Role.Worker),
        ("Carlos Ruiz", "3333", Role.Admin)
    ];

    private readonly IDbConnectionFactory _connectionFactory;
    private readonly string _connectionString;
    private readonly IPasswordHasher _passwordHasher;
    private readonly bool _seedDemoData;
    private readonly string _adminPassword;

    public DatabaseInitializer(
        IDbConnectionFactory connectionFactory,
        string connectionString,
        IPasswordHasher passwordHasher,
        bool seedDemoData,
        string adminPassword)
    {
        _connectionFactory = connectionFactory;
        _connectionString = connectionString;
        _passwordHasher = passwordHasher;
        _seedDemoData = seedDemoData;
        _adminPassword = adminPassword;
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
        await EnsureAdminUserAsync(connection, transaction);

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
                INSERT INTO centers (id, name)
                VALUES (@id, @name)
                ON CONFLICT (id) DO NOTHING
                """,
                new { id = DemoCenterId, name = "Peluquería Sol" },
                transaction));

        await connection.ExecuteAsync(
            new CommandDefinition(
                "SELECT setval(pg_get_serial_sequence('centers', 'id'), GREATEST((SELECT COALESCE(MAX(id), 0) FROM centers), 1))",
                transaction: transaction));
    }

    private async Task EnsureAdminUserAsync(
        Npgsql.NpgsqlConnection connection,
        System.Data.Common.DbTransaction transaction)
    {
        var exists = await connection.ExecuteScalarAsync<bool>(
            new CommandDefinition(
                "SELECT EXISTS (SELECT 1 FROM center_users WHERE username = 'admin')",
                transaction: transaction));

        if (exists)
        {
            return;
        }

        await connection.ExecuteAsync(
            new CommandDefinition(
                """
                INSERT INTO center_users (center_id, username, password_hash)
                VALUES (@centerId, 'admin', @passwordHash)
                """,
                new { centerId = DemoCenterId, passwordHash = _passwordHasher.Hash(_adminPassword) },
                transaction));
    }

    private async Task EnsureDemoDataAsync(
        Npgsql.NpgsqlConnection connection,
        System.Data.Common.DbTransaction transaction)
    {
        var workerCount = await connection.ExecuteScalarAsync<int>(
            new CommandDefinition(
                "SELECT COUNT(*) FROM workers WHERE center_id = @centerId",
                new { centerId = DemoCenterId },
                transaction));

        if (workerCount > 0)
        {
            return;
        }

        var workerIds = new List<int>(DemoWorkers.Length);

        foreach (var (fullName, pin, role) in DemoWorkers)
        {
            var id = await connection.ExecuteScalarAsync<int>(
                new CommandDefinition(
                    """
                    INSERT INTO workers (center_id, full_name, pin_hash, role)
                    VALUES (@centerId, @fullName, @pinHash, @role)
                    RETURNING id
                    """,
                    new
                    {
                        centerId = DemoCenterId,
                        fullName,
                        pinHash = _passwordHasher.Hash(pin),
                        role = role.ToString()
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
