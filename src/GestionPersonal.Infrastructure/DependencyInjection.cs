using GestionPersonal.Application.Abstractions;
using GestionPersonal.Infrastructure.Persistence.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace GestionPersonal.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<IWorkerRepository, InMemoryWorkerRepository>();

        return services;
    }
}
