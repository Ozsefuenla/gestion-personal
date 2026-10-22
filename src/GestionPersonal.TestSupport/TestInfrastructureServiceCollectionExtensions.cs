using GestionPersonal.Application.Abstractions;
using GestionPersonal.Infrastructure.Security;
using GestionPersonal.TestSupport.Fakes;
using Microsoft.Extensions.DependencyInjection;

namespace GestionPersonal.TestSupport;

public static class TestInfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddInMemoryTestInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<IWorkerRepository, InMemoryWorkerRepository>();
        services.AddSingleton<ITimeEntryRepository, InMemoryTimeEntryRepository>();
        services.AddSingleton<IVacationRepository, InMemoryVacationRepository>();
        services.AddSingleton<ICenterRepository, InMemoryCenterRepository>();
        services.AddSingleton<ICenterUserRepository, InMemoryCenterUserRepository>();
        services.AddSingleton<IPasswordHasher, BcryptPasswordHasher>();
        services.AddSingleton<ILoginAttemptTracker, InMemoryLoginAttemptTracker>();

        services.AddSingleton<FakeEventCollector>();
        services.AddSingleton<IEventCollector>(provider => provider.GetRequiredService<FakeEventCollector>());
        services.AddSingleton<IExceptionReporter>(provider => provider.GetRequiredService<FakeEventCollector>());
        services.AddSingleton<IRequestMetricsRecorder>(provider => provider.GetRequiredService<FakeEventCollector>());

        services.AddSingleton<FakeMonitoringRepository>();
        services.AddSingleton<IMonitoringRepository>(provider => provider.GetRequiredService<FakeMonitoringRepository>());

        return services;
    }
}
