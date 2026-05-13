using Cleaning.AI;
using Cleaning.Core.Contracts;
using Cleaning.Core.DTOs;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

namespace Cleaning.Infra.AI;

internal sealed class OpenAiMaintenanceAlertAnalyzer(
    IChatClient chatClient,
    ILogger<OpenAiMaintenanceAlertAnalyzer> logger) : IMaintenanceAlertAnalyzer
{
    public bool IsConfigured => true;

    public async Task<AiMaintenanceAnalysisResult?> AnalyzeAsync(
        IReadOnlyCollection<MaintenanceAlertDto> alerts,
        CancellationToken cancellationToken)
    {
        if (alerts.Count == 0)
        {
            return null;
        }

        var response = await chatClient.GetResponseAsync<MaintenanceAlertAgentResponse>(
            MaintenanceAlertAgent.CreateMessages(alerts),
            options: new ChatOptions
            {
                Temperature = 0,
                MaxOutputTokens = 500
            },
            cancellationToken: cancellationToken);

        if (!response.TryGetResult(out var result) || result is null)
        {
            logger.LogWarning("A resposta da IA nao retornou um JSON valido para o painel de manutencao.");
            return null;
        }

        var highlights = result.Highlights
            .Where(item => !string.IsNullOrWhiteSpace(item))
            .Select(item => item.Trim())
            .Take(4)
            .ToArray();

        return new AiMaintenanceAnalysisResult(
            result.Summary.Trim(),
            result.Recommendation.Trim(),
            result.Priority,
            highlights.Length == 0 ? ["Sem destaques adicionais."] : highlights);
    }
}
