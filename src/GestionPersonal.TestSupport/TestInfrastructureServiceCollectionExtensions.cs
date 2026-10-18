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

        return services;
    }
}
