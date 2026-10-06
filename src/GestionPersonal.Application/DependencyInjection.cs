using FluentValidation;
using GestionPersonal.Application.Workers;
using Microsoft.Extensions.DependencyInjection;

namespace GestionPersonal.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ListWorkersQueryHandler>();
        services.AddScoped<CreateWorkerCommandHandler>();

        services.AddValidatorsFromAssemblyContaining<CreateWorkerCommandValidator>();

        return services;
    }
}
