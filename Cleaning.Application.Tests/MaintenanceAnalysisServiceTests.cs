using Cleaning.Core.Contracts;
using Cleaning.Core.DTOs;
using Cleaning.Core.Enums;
using Microsoft.Extensions.DependencyInjection;

namespace Cleaning.Application.Tests;

public sealed class MaintenanceAnalysisServiceTests
{
    [Fact]
    public async Task AnalyzeAsync_ShouldUseFallbackWhenAiIsUnavailable()
    {
        var alerts = new MaintenanceAlertsResponse(
            new DateTimeOffset(2026, 5, 13, 10, 0, 0, TimeSpan.Zero),
            5,
            [
                new MaintenanceAlertDto(
                    Guid.NewGuid(),
                    "Calibrar reciclagem de agua",
                    "Revisao urgente do sistema de reaproveitamento da ala norte.",
                    new DateOnly(2026, 5, 1),
                    -12,
                    MaintenanceTaskStatus.Overdue,
                    MaintenancePriority.Critical,
                    "Calibrar reciclagem de agua esta atrasada ha 12 dia(s).")
            ]);

        var provider = CreateProvider(new StubAlertService(alerts), new StubAlertAnalyzer(false, null));
        var service = provider.GetRequiredService<IMaintenanceAnalysisService>();

        var analysis = await service.AnalyzeAsync(CancellationToken.None);

        Assert.True(analysis.UsedFallback);
        Assert.False(analysis.AiConfigured);
        Assert.Equal("Fallback", analysis.Source);
        Assert.Equal(MaintenancePriority.Critical, analysis.Priority);
    }

    [Fact]
    public async Task AnalyzeAsync_ShouldPreferAiResultWhenAvailable()
    {
        var alerts = new MaintenanceAlertsResponse(
            new DateTimeOffset(2026, 5, 13, 10, 0, 0, TimeSpan.Zero),
            5,
            [
                new MaintenanceAlertDto(
                    Guid.NewGuid(),
                    "Higienizar alas medicas",
                    "Esterilizacao de cabines e descarte de residuos biologicos.",
                    new DateOnly(2026, 5, 13),
                    0,
                    MaintenanceTaskStatus.Pending,
                    MaintenancePriority.High,
                    "Higienizar alas medicas precisa ser executada hoje.")
            ]);

        var aiResult = new AiMaintenanceAnalysisResult(
            "Existe uma tarefa de alta urgencia aguardando execucao imediata.",
            "Despache a equipe medica de limpeza antes do fim do turno atual.",
            MaintenancePriority.High,
            ["Foco total na ala medica.", "Nao iniciar novas rotinas antes da conclusao."]);

        var provider = CreateProvider(new StubAlertService(alerts), new StubAlertAnalyzer(true, aiResult));
        var service = provider.GetRequiredService<IMaintenanceAnalysisService>();

        var analysis = await service.AnalyzeAsync(CancellationToken.None);

        Assert.False(analysis.UsedFallback);
        Assert.True(analysis.AiConfigured);
        Assert.Equal("IA", analysis.Source);
        Assert.Equal(aiResult.Recommendation, analysis.Recommendation);
    }

    private static IServiceProvider CreateProvider(IMaintenanceAlertService alertService, IMaintenanceAlertAnalyzer analyzer)
    {
        var services = new ServiceCollection();
        services.AddApplication();
        services.AddSingleton(alertService);
        services.AddSingleton(analyzer);

        return services.BuildServiceProvider();
    }
}
