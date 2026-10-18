using GestionPersonal.Application.Abstractions;
using GestionPersonal.Infrastructure.Persistence.Postgres;
using GestionPersonal.Infrastructure.Persistence.Postgres.Repositories;
using GestionPersonal.Infrastructure.Persistence.Repositories;
using GestionPersonal.Infrastructure.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GestionPersonal.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var provider = configuration["Persistence:Provider"] ?? "Postgres";

        if (string.Equals(provider, "InMemory", StringComparison.OrdinalIgnoreCase))
        {
            return services.AddInMemoryInfrastructure();
        }

        var connectionString = configuration.GetConnectionString("Default");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Falta la cadena de conexión 'ConnectionStrings:Default' para PostgreSQL.");
        }

        var connectionFactory = new NpgsqlConnectionFactory(connectionString);

        services.AddSingleton<IDbConnectionFactory>(connectionFactory);
        services.AddScoped<IWorkerRepository, PostgresWorkerRepository>();
        services.AddScoped<ITimeEntryRepository, PostgresTimeEntryRepository>();
        services.AddScoped<IVacationRepository, PostgresVacationRepository>();
        services.AddScoped<ICenterRepository, PostgresCenterRepository>();
        services.AddScoped<ICenterUserRepository, PostgresCenterUserRepository>();
        services.AddSingleton<IPasswordHasher, BcryptPasswordHasher>();
        services.AddSingleton<ILoginAttemptTracker, InMemoryLoginAttemptTracker>();

        var seedDemoData = bool.TryParse(configuration["Persistence:SeedDemoData"], out var seed) && seed;
        var adminPassword = configuration["Persistence:AdminPassword"] ?? "admin";

        services.AddSingleton(serviceProvider => new DatabaseInitializer(
            connectionFactory,
            connectionString,
            serviceProvider.GetRequiredService<IPasswordHasher>(),
            seedDemoData,
            adminPassword));

        return services;
    }

    public static IServiceCollection AddInMemoryInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<IWorkerRepository, InMemoryWorkerRepository>();
        services.AddSingleton<ITimeEntryRepository, InMemoryTimeEntryRepository>();
        services.AddSingleton<IVacationRepository, InMemoryVacationRepository>();
        services.AddSingleton<ICenterRepository, InMemoryCenterRepository>();
        services.AddSingleton<ICenterUserRepository, InMemoryCenterUserRepository>();
        services.AddSingleton<IPasswordHasher, BcryptPasswordHasher>();
        services.AddSingleton<ILoginAttemptTracker, InMemoryLoginAttemptTracker>();

        return services;
    }
}
