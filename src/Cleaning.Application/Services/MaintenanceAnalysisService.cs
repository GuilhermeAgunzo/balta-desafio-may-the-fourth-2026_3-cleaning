using Cleaning.Core.Contracts;
using Cleaning.Core.DTOs;
using Cleaning.Core.Enums;

namespace Cleaning.Application.Services;

internal sealed class MaintenanceAnalysisService(
    IMaintenanceAlertService alertService,
    IMaintenanceAlertAnalyzer alertAnalyzer) : IMaintenanceAnalysisService
{
    public async Task<MaintenanceAnalysisDto> AnalyzeAsync(CancellationToken cancellationToken)
    {
        var alerts = await alertService.GetAlertsAsync(cancellationToken);

        if (alerts.Items.Count > 0)
        {
            var aiResult = await alertAnalyzer.AnalyzeAsync(alerts.Items, cancellationToken);
            if (aiResult is not null)
            {
                return new MaintenanceAnalysisDto(
                    alerts.GeneratedAt,
                    true,
                    false,
                    "IA",
                    aiResult.Priority,
                    aiResult.Summary,
                    aiResult.Recommendation,
                    aiResult.Highlights);
            }
        }

        return BuildFallback(alerts);
    }

    private MaintenanceAnalysisDto BuildFallback(MaintenanceAlertsResponse alerts)
    {
        if (alerts.Items.Count == 0)
        {
            return new MaintenanceAnalysisDto(
                alerts.GeneratedAt,
                alertAnalyzer.IsConfigured,
                true,
                "Fallback",
                MaintenancePriority.Low,
                "Nenhum alerta ativo foi detectado no painel de manutencao.",
                "Mantenha a rotina atual e acompanhe as proximas execucoes planejadas.",
                ["Sistema estavel.", "Sem tarefas vencidas ou imminentes."]);
        }

        var highestPriority = alerts.Items.MaxBy(alert => MaintenanceMapping.GetPriorityWeight(alert.Priority))!.Priority;
        var overdueCount = alerts.Items.Count(alert => alert.Status == MaintenanceTaskStatus.Overdue);
        var dueTodayCount = alerts.Items.Count(alert => alert.DaysUntilDue == 0);

        var summary = overdueCount > 0
            ? $"Foram encontrados {overdueCount} alerta(s) atrasado(s) e {alerts.Items.Count} alerta(s) ativo(s) no total."
            : $"Existem {alerts.Items.Count} alerta(s) de manutencao no radar, com {dueTodayCount} tarefa(s) vencendo hoje.";

        var recommendation = highestPriority switch
        {
            MaintenancePriority.Critical => "Priorize imediatamente as tarefas mais atrasadas antes de adicionar novas demandas ao ciclo.",
            MaintenancePriority.High => "Execute hoje as tarefas sinalizadas como mais urgentes e reavalie o painel apos a conclusao.",
            MaintenancePriority.Medium => "Organize a agenda dos proximos dias para impedir que os alertas avancem para atraso.",
            _ => "Mantenha acompanhamento preventivo e conclua as tarefas dentro da janela recomendada."
        };

        var highlights = alerts.Items
            .Take(3)
            .Select(alert => $"{alert.TaskName}: {alert.Message}")
            .ToArray();

        return new MaintenanceAnalysisDto(
            alerts.GeneratedAt,
            alertAnalyzer.IsConfigured,
            true,
            "Fallback",
            highestPriority,
            summary,
            recommendation,
            highlights);
    }
}
