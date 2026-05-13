using Cleaning.Core.Contracts;
using Cleaning.Core.DTOs;
using Cleaning.Core.Enums;
using Microsoft.Extensions.DependencyInjection;

namespace Cleaning.Application.Tests;

public sealed class MaintenanceTaskServiceTests
{
    [Fact]
    public async Task CreateAsync_ShouldCreatePendingTask()
    {
        var provider = CreateProvider(new FixedTimeProvider(new DateTimeOffset(2026, 5, 4, 12, 0, 0, TimeSpan.Zero)));
        var service = provider.GetRequiredService<IMaintenanceTaskService>();

        var created = await service.CreateAsync(
            new CreateMaintenanceTaskRequest
            {
                Name = "Limpar droides",
                Description = "Remover poeira de todos os compartimentos dos droides de servico.",
                RecurrenceInterval = 2,
                RecurrenceUnit = RecurrenceUnit.Weeks,
                FirstExecutionDate = new DateOnly(2026, 5, 10)
            },
            CancellationToken.None);

        Assert.Equal(MaintenanceTaskStatus.Pending, created.Status);
        Assert.Equal(new DateOnly(2026, 5, 10), created.NextExecutionDate);
        Assert.Equal("2 semanas", created.RecurrenceLabel);
    }

    [Fact]
    public async Task CompleteAsync_ShouldRecalculateNextExecutionDate()
    {
        var timeProvider = new FixedTimeProvider(new DateTimeOffset(2026, 5, 10, 8, 0, 0, TimeSpan.Zero));
        var provider = CreateProvider(timeProvider);
        var service = provider.GetRequiredService<IMaintenanceTaskService>();

        var created = await service.CreateAsync(
            new CreateMaintenanceTaskRequest
            {
                Name = "Trocar filtro do hangar",
                Description = "Substituir o filtro principal do sistema de ventilacao do hangar.",
                RecurrenceInterval = 1,
                RecurrenceUnit = RecurrenceUnit.Months,
                FirstExecutionDate = new DateOnly(2026, 5, 10)
            },
            CancellationToken.None);

        var completed = await service.CompleteAsync(created.Id, CancellationToken.None);

        Assert.NotNull(completed);
        Assert.Equal(new DateOnly(2026, 6, 10), completed!.NextExecutionDate);
        Assert.Equal(new DateOnly(2026, 5, 10), completed.LastCompletedOn);
    }

    [Fact]
    public async Task ListAsync_ShouldFilterOverdueTasks()
    {
        var timeProvider = new FixedTimeProvider(new DateTimeOffset(2026, 5, 10, 9, 0, 0, TimeSpan.Zero));
        var provider = CreateProvider(timeProvider);
        var service = provider.GetRequiredService<IMaintenanceTaskService>();

        await service.CreateAsync(
            new CreateMaintenanceTaskRequest
            {
                Name = "Inspecionar escudos",
                Description = "Checar a integridade dos emissores de escudo da nave principal.",
                RecurrenceInterval = 7,
                RecurrenceUnit = RecurrenceUnit.Days,
                FirstExecutionDate = new DateOnly(2026, 5, 8)
            },
            CancellationToken.None);

        await service.CreateAsync(
            new CreateMaintenanceTaskRequest
            {
                Name = "Polir cockpit",
                Description = "Aplicar limpeza de acabamento no cockpit do esquadrao principal.",
                RecurrenceInterval = 1,
                RecurrenceUnit = RecurrenceUnit.Weeks,
                FirstExecutionDate = new DateOnly(2026, 5, 12)
            },
            CancellationToken.None);

        var overdue = await service.ListAsync(MaintenanceTaskFilter.Overdue, CancellationToken.None);

        Assert.Single(overdue);
        Assert.Equal("Inspecionar escudos", overdue[0].Name);
        Assert.Equal(MaintenanceTaskStatus.Overdue, overdue[0].Status);
    }

    private static IServiceProvider CreateProvider(FixedTimeProvider timeProvider)
    {
        var services = new ServiceCollection();
        services.AddApplication();
        services.AddSingleton<IMaintenanceTaskRepository, InMemoryMaintenanceTaskRepository>();
        services.AddSingleton<TimeProvider>(timeProvider);

        return services.BuildServiceProvider();
    }
}
