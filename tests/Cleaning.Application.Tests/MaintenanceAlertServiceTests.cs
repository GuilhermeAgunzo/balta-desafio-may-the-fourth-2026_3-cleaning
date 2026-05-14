using Cleaning.Application.Options;
using Cleaning.Core.Contracts;
using Cleaning.Core.Entities;
using Cleaning.Core.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Cleaning.Application.Tests;

public sealed class MaintenanceAlertServiceTests
{
    [Fact]
    public async Task GetAlertsAsync_ShouldIncludeOverdueTasksRespectLeadWindowAndSortMappedAlerts()
    {
        var now = new DateTimeOffset(2026, 5, 13, 10, 0, 0, TimeSpan.Zero);
        var referenceDate = DateOnly.FromDateTime(now.UtcDateTime);
        var provider = CreateProvider(now, leadWindowInDays: 5);
        var repository = provider.GetRequiredService<IMaintenanceTaskRepository>();
        var service = provider.GetRequiredService<IMaintenanceAlertService>();

        await AddTaskAsync(repository, "Calibrar reciclagem de agua", referenceDate.AddDays(-8), now);
        await AddTaskAsync(repository, "Revisar filtros do hangar", referenceDate.AddDays(-1), now);
        await AddTaskAsync(repository, "Higienizar alas medicas", referenceDate, now);
        await AddTaskAsync(repository, "Inspecionar escudos", referenceDate.AddDays(2), now);
        await AddTaskAsync(repository, "Ajustar sensores", referenceDate.AddDays(5), now);
        await AddTaskAsync(repository, "Polir cockpit", referenceDate.AddDays(5), now);
        await AddTaskAsync(repository, "Ignorar manutencao futura", referenceDate.AddDays(6), now);

        var response = await service.GetAlertsAsync(CancellationToken.None);

        Assert.Equal(5, response.LeadWindowInDays);
        Assert.Equal(
            [
                "Calibrar reciclagem de agua",
                "Revisar filtros do hangar",
                "Higienizar alas medicas",
                "Inspecionar escudos",
                "Ajustar sensores",
                "Polir cockpit"
            ],
            response.Items.Select(alert => alert.TaskName).ToArray());

        Assert.Collection(
            response.Items,
            critical =>
            {
                Assert.Equal(-8, critical.DaysUntilDue);
                Assert.Equal(MaintenanceTaskStatus.Overdue, critical.Status);
                Assert.Equal(MaintenancePriority.Critical, critical.Priority);
                Assert.Equal("Calibrar reciclagem de agua esta atrasada ha 8 dia(s).", critical.Message);
            },
            overdue =>
            {
                Assert.Equal(-1, overdue.DaysUntilDue);
                Assert.Equal(MaintenanceTaskStatus.Overdue, overdue.Status);
                Assert.Equal(MaintenancePriority.High, overdue.Priority);
                Assert.Equal("Revisar filtros do hangar esta atrasada ha 1 dia(s).", overdue.Message);
            },
            dueToday =>
            {
                Assert.Equal(0, dueToday.DaysUntilDue);
                Assert.Equal(MaintenanceTaskStatus.Pending, dueToday.Status);
                Assert.Equal(MaintenancePriority.High, dueToday.Priority);
                Assert.Equal("Higienizar alas medicas precisa ser executada hoje.", dueToday.Message);
            },
            medium =>
            {
                Assert.Equal(2, medium.DaysUntilDue);
                Assert.Equal(MaintenancePriority.Medium, medium.Priority);
                Assert.Equal("Inspecionar escudos vence em 2 dia(s).", medium.Message);
            },
            lowAlpha =>
            {
                Assert.Equal(5, lowAlpha.DaysUntilDue);
                Assert.Equal(MaintenancePriority.Low, lowAlpha.Priority);
                Assert.Equal("Ajustar sensores vence em 5 dia(s).", lowAlpha.Message);
            },
            lowBeta =>
            {
                Assert.Equal(5, lowBeta.DaysUntilDue);
                Assert.Equal(MaintenancePriority.Low, lowBeta.Priority);
                Assert.Equal("Polir cockpit vence em 5 dia(s).", lowBeta.Message);
            });

        Assert.DoesNotContain(response.Items, alert => alert.TaskName == "Ignorar manutencao futura");
    }

    private static IServiceProvider CreateProvider(DateTimeOffset current, int leadWindowInDays)
    {
        var services = new ServiceCollection();
        services.AddApplication();
        services.AddSingleton<IMaintenanceTaskRepository, InMemoryMaintenanceTaskRepository>();
        services.AddSingleton<TimeProvider>(new FixedTimeProvider(current));
        services.AddSingleton<IOptions<MaintenanceAlertOptions>>(
            Microsoft.Extensions.Options.Options.Create(new MaintenanceAlertOptions
            {
                LeadWindowInDays = leadWindowInDays
            }));

        return services.BuildServiceProvider();
    }

    private static Task AddTaskAsync(
        IMaintenanceTaskRepository repository,
        string name,
        DateOnly nextExecutionDate,
        DateTimeOffset createdAt)
        => repository.AddAsync(
            new MaintenanceTask(
                name,
                $"Descricao detalhada para {name}.",
                1,
                RecurrenceUnit.Weeks,
                nextExecutionDate,
                createdAt),
            CancellationToken.None);
}
