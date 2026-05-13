using Cleaning.Core.Contracts;
using Cleaning.Infra.AI;
using Cleaning.Infra.Data;
using Cleaning.Infra.Options;
using Cleaning.Infra.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenAI.Chat;

namespace Cleaning.Infra;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("CleaningDb") ?? "Data Source=cleaning.db";

        services.Configure<OpenAiOptions>(configuration.GetSection(OpenAiOptions.SectionName));

        services.AddDbContext<CleaningDbContext>(options => options.UseSqlite(connectionString));
        services.AddScoped<IMaintenanceTaskRepository, MaintenanceTaskRepository>();

        services.AddScoped<IMaintenanceAlertAnalyzer>(serviceProvider =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<OpenAiOptions>>().Value;
            if (string.IsNullOrWhiteSpace(options.ApiKey))
            {
                return new DisabledMaintenanceAlertAnalyzer();
            }

            var chatClient = new ChatClient(options.ModelId, options.ApiKey).AsIChatClient();
            var logger = serviceProvider.GetRequiredService<ILogger<OpenAiMaintenanceAlertAnalyzer>>();

            return new OpenAiMaintenanceAlertAnalyzer(chatClient, logger);
        });

        return services;
    }
}
