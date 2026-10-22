using FluentValidation;
using GestionPersonal.Application.Abstractions;
using GestionPersonal.Application.Account;
using GestionPersonal.Application.Centers;
using GestionPersonal.Application.Common;
using GestionPersonal.Application.Monitoring;
using GestionPersonal.Application.Platform;
using GestionPersonal.Application.Services;
using GestionPersonal.Application.TimeEntries;
using GestionPersonal.Application.Vacations;
using GestionPersonal.Application.Workers;
using Microsoft.Extensions.DependencyInjection;

namespace GestionPersonal.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IRequestValidator, RequestValidator>();

        services.AddSingleton<ITimeEntryChangeNotifier, TimeEntryChangeNotifier>();

        services.AddScoped<ICentersService, CentersService>();
        services.AddScoped<IWorkersService, WorkersService>();
        services.AddScoped<ITimeEntriesService, TimeEntriesService>();
        services.AddScoped<IVacationsService, VacationsService>();
        services.AddScoped<IPlatformService, PlatformService>();
        services.AddScoped<IAccountService, AccountService>();
        services.AddScoped<IMonitoringService, MonitoringService>();

        services.AddScoped<ListWorkersQueryHandler>();
        services.AddScoped<ListWorkersByCenterQueryHandler>();
        services.AddScoped<ListWorkersForAdminQueryHandler>();
        services.AddScoped<ListCentersQueryHandler>();
        services.AddScoped<CenterLoginCommandHandler>();
        services.AddScoped<CreateWorkerCommandHandler>();
        services.AddScoped<GetWorkersStatusQueryHandler>();
        services.AddScoped<GetWorkerStatusQueryHandler>();
        services.AddScoped<LoginCommandHandler>();
        services.AddScoped<ChangePinCommandHandler>();
        services.AddScoped<RenameWorkerCommandHandler>();
        services.AddScoped<ResetWorkerPinCommandHandler>();
        services.AddScoped<DeactivateWorkerCommandHandler>();
        services.AddScoped<ActivateWorkerCommandHandler>();
        services.AddScoped<StartTimeEntryCommandHandler>();
        services.AddScoped<PauseTimeEntryCommandHandler>();
        services.AddScoped<EndTimeEntryCommandHandler>();
        services.AddScoped<ResumeTimeEntryCommandHandler>();
        services.AddScoped<TodaySummaryQueryHandler>();
        services.AddScoped<MonthlySummaryQueryHandler>();
        services.AddScoped<QuickClockCommandHandler>();
        services.AddScoped<AddManualTimeEntryCommandHandler>();
        services.AddScoped<EditActiveTimeEntryCommandHandler>();
        services.AddScoped<DeleteActiveTimeEntryCommandHandler>();
        services.AddScoped<RequestVacationCommandHandler>();
        services.AddScoped<ListVacationsQueryHandler>();
        services.AddScoped<DeleteVacationCommandHandler>();
        services.AddScoped<ApproveVacationCommandHandler>();
        services.AddScoped<RejectVacationCommandHandler>();
        services.AddScoped<ListCenterVacationsQueryHandler>();
        services.AddScoped<ListAllCentersQueryHandler>();
        services.AddScoped<CreateCenterCommandHandler>();
        services.AddScoped<SetCenterActiveCommandHandler>();
        services.AddScoped<CreateCenterAdminCommandHandler>();
        services.AddScoped<ListCenterUsersQueryHandler>();
        services.AddScoped<CreateCenterUserCommandHandler>();
        services.AddScoped<SetCenterUserActiveCommandHandler>();
        services.AddScoped<ResetCenterUserPasswordCommandHandler>();
        services.AddScoped<ChangePasswordCommandHandler>();
        services.AddScoped<GetPlatformOverviewQueryHandler>();
        services.AddScoped<ListExceptionsQueryHandler>();
        services.AddScoped<GetErrorRateSeriesQueryHandler>();

        services.AddValidatorsFromAssemblyContaining<CreateWorkerCommandValidator>();

        return services;
    }
}
