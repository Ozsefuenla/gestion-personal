using GestionPersonal.Application.Abstractions;
using GestionPersonal.Infrastructure.Persistence.Postgres;
using GestionPersonal.Infrastructure.Telemetry;
using GestionPersonal.TestSupport;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace GestionPersonal.IntegrationTests;

public sealed class GestionPersonalWebAppFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IWorkerRepository>();
            services.RemoveAll<ITimeEntryRepository>();
            services.RemoveAll<IVacationRepository>();
            services.RemoveAll<ICenterRepository>();
            services.RemoveAll<ICenterUserRepository>();
            services.RemoveAll<IMonitoringRepository>();
            services.RemoveAll<IDbConnectionFactory>();
            services.RemoveAll<DatabaseInitializer>();

            foreach (var descriptor in services
                .Where(descriptor => descriptor.ServiceType == typeof(IHostedService)
                    && (descriptor.ImplementationType == typeof(TelemetryWriter)
                        || descriptor.ImplementationType == typeof(RetentionJob)))
                .ToList())
            {
                services.Remove(descriptor);
            }

            services.AddInMemoryTestInfrastructure();
        });
    }
}
