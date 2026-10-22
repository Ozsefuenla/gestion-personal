using GestionPersonal.Application.Abstractions;
using GestionPersonal.Infrastructure.Persistence.Postgres;
using GestionPersonal.Infrastructure.Persistence.Postgres.Repositories;
using GestionPersonal.Infrastructure.Security;
using GestionPersonal.Infrastructure.Telemetry;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace GestionPersonal.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
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
        services.AddScoped<IMonitoringRepository, PostgresMonitoringRepository>();
        services.AddSingleton<IPasswordHasher, BcryptPasswordHasher>();
        services.AddSingleton<ILoginAttemptTracker, InMemoryLoginAttemptTracker>();

        var telemetryOptions = TelemetryOptions.FromConfiguration(key => configuration[key]);
        services.AddSingleton(telemetryOptions);
        services.AddSingleton<TelemetryCollector>();
        services.AddSingleton<IEventCollector>(provider => provider.GetRequiredService<TelemetryCollector>());
        services.AddSingleton<IExceptionReporter>(provider => provider.GetRequiredService<TelemetryCollector>());
        services.AddSingleton<IRequestMetricsRecorder>(provider => provider.GetRequiredService<TelemetryCollector>());
        services.AddSingleton<ILoggerProvider, TelemetryLoggerProvider>();
        services.AddHostedService<TelemetryWriter>();
        services.AddHostedService<RetentionJob>();
        services.AddHostedService<TelemetryBusinessSubscriber>();

        var seedDemoData = bool.TryParse(configuration["Persistence:SeedDemoData"], out var seed) && seed;
        var credentials = SeedCredentials.FromConfiguration(key => configuration[key]);

        services.AddSingleton(serviceProvider => new DatabaseInitializer(
            connectionFactory,
            connectionString,
            serviceProvider.GetRequiredService<IPasswordHasher>(),
            seedDemoData,
            credentials));

        return services;
    }
}
