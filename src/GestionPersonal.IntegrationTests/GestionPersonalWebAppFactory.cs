using GestionPersonal.Application.Abstractions;
using GestionPersonal.Infrastructure.Persistence.Postgres;
using GestionPersonal.TestSupport;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

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
            services.RemoveAll<IDbConnectionFactory>();
            services.RemoveAll<DatabaseInitializer>();

            services.AddInMemoryTestInfrastructure();
        });
    }
}
