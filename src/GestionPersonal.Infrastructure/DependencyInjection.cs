using GestionPersonal.Application.Abstractions;
using GestionPersonal.Infrastructure.Persistence.Repositories;
using GestionPersonal.Infrastructure.Security;
using Microsoft.Extensions.DependencyInjection;

namespace GestionPersonal.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<IWorkerRepository, InMemoryWorkerRepository>();
        services.AddSingleton<ITimeEntryRepository, InMemoryTimeEntryRepository>();
        services.AddSingleton<ICenterRepository, InMemoryCenterRepository>();
        services.AddSingleton<ICenterUserRepository, InMemoryCenterUserRepository>();
        services.AddSingleton<IPasswordHasher, BcryptPasswordHasher>();
        services.AddSingleton<ILoginAttemptTracker, InMemoryLoginAttemptTracker>();

        return services;
    }
}
