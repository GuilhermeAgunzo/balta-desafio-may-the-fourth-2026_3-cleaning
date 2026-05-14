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

    [Fact]
    public async Task AnalyzeAsync_ShouldSkipAiWhenThereAreNoAlerts()
    {
        var alerts = new MaintenanceAlertsResponse(
            new DateTimeOffset(2026, 5, 13, 10, 0, 0, TimeSpan.Zero),
            5,
            []);

        var analyzer = new StubAlertAnalyzer(true, null);
        var provider = CreateProvider(new StubAlertService(alerts), analyzer);
        var service = provider.GetRequiredService<IMaintenanceAnalysisService>();

        var analysis = await service.AnalyzeAsync(CancellationToken.None);

        Assert.True(analysis.UsedFallback);
        Assert.True(analysis.AiConfigured);
        Assert.Equal("Fallback", analysis.Source);
        Assert.Equal(MaintenancePriority.Low, analysis.Priority);
        Assert.Equal("Nenhum alerta ativo foi detectado no painel de manutencao.", analysis.Summary);
        Assert.Equal(0, analyzer.CallCount);
        Assert.Equal(
            ["Sistema estavel.", "Sem tarefas vencidas ou imminentes."],
            analysis.Highlights);
    }

    [Fact]
    public async Task AnalyzeAsync_ShouldBuildFallbackForMixedAlertsUsingHighestPriority()
    {
        var alerts = new MaintenanceAlertsResponse(
            new DateTimeOffset(2026, 5, 13, 10, 0, 0, TimeSpan.Zero),
            5,
            [
                new MaintenanceAlertDto(
                    Guid.NewGuid(),
                    "Calibrar reciclagem de agua",
                    "Revisao urgente do sistema de reaproveitamento da ala norte.",
                    new DateOnly(2026, 5, 5),
                    -8,
                    MaintenanceTaskStatus.Overdue,
                    MaintenancePriority.Critical,
                    "Calibrar reciclagem de agua esta atrasada ha 8 dia(s)."),
                new MaintenanceAlertDto(
                    Guid.NewGuid(),
                    "Higienizar alas medicas",
                    "Esterilizacao de cabines e descarte de residuos biologicos.",
                    new DateOnly(2026, 5, 13),
                    0,
                    MaintenanceTaskStatus.Pending,
                    MaintenancePriority.High,
                    "Higienizar alas medicas precisa ser executada hoje."),
                new MaintenanceAlertDto(
                    Guid.NewGuid(),
                    "Revisar filtros do hangar",
                    "Troca preventiva dos filtros do sistema de ventilacao principal.",
                    new DateOnly(2026, 5, 14),
                    1,
                    MaintenanceTaskStatus.Pending,
                    MaintenancePriority.Medium,
                    "Revisar filtros do hangar vence em 1 dia(s).")
            ]);

        var analyzer = new StubAlertAnalyzer(true, null);
        var provider = CreateProvider(new StubAlertService(alerts), analyzer);
        var service = provider.GetRequiredService<IMaintenanceAnalysisService>();

        var analysis = await service.AnalyzeAsync(CancellationToken.None);

        Assert.True(analysis.UsedFallback);
        Assert.True(analysis.AiConfigured);
        Assert.Equal("Fallback", analysis.Source);
        Assert.Equal(MaintenancePriority.Critical, analysis.Priority);
        Assert.Equal("Foram encontrados 1 alerta(s) atrasado(s) e 3 alerta(s) ativo(s) no total.", analysis.Summary);
        Assert.Equal(
            "Priorize imediatamente as tarefas mais atrasadas antes de adicionar novas demandas ao ciclo.",
            analysis.Recommendation);
        Assert.Equal(
            [
                "Calibrar reciclagem de agua: Calibrar reciclagem de agua esta atrasada ha 8 dia(s).",
                "Higienizar alas medicas: Higienizar alas medicas precisa ser executada hoje.",
                "Revisar filtros do hangar: Revisar filtros do hangar vence em 1 dia(s)."
            ],
            analysis.Highlights);
        Assert.Equal(1, analyzer.CallCount);
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
