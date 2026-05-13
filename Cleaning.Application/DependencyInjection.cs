using Cleaning.Application.Services;
using Cleaning.Core.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace Cleaning.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddSingleton(TimeProvider.System);
        services.AddScoped<IMaintenanceTaskService, MaintenanceTaskService>();
        services.AddScoped<IMaintenanceAlertService, MaintenanceAlertService>();
        services.AddScoped<IMaintenanceAnalysisService, MaintenanceAnalysisService>();

        return services;
    }
}
