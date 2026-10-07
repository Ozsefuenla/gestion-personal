using FluentValidation;
using GestionPersonal.Application.TimeEntries;
using GestionPersonal.Application.Workers;
using Microsoft.Extensions.DependencyInjection;

namespace GestionPersonal.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ListWorkersQueryHandler>();
        services.AddScoped<CreateWorkerCommandHandler>();
        services.AddScoped<GetWorkersStatusQueryHandler>();
        services.AddScoped<StartTimeEntryCommandHandler>();
        services.AddScoped<PauseTimeEntryCommandHandler>();
        services.AddScoped<EndTimeEntryCommandHandler>();
        services.AddScoped<ResumeTimeEntryCommandHandler>();
        services.AddScoped<TodaySummaryQueryHandler>();

        services.AddValidatorsFromAssemblyContaining<CreateWorkerCommandValidator>();

        return services;
    }
}
