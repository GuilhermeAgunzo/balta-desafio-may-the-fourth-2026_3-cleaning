using System.Text.Json;
using Cleaning.Core.DTOs;
using Cleaning.Core.Enums;
using Microsoft.Extensions.AI;

namespace Cleaning.AI;

public static class MaintenanceAlertAgent
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public const string SystemPrompt = """
        Voce e um analista de manutencao residencial com tom profissional e objetivo.
        Analise os alertas recebidos e retorne:
        - um resumo executivo curto;
        - uma recomendacao pratica;
        - a prioridade consolidada;
        - uma lista curta de destaques.

        Regras obrigatorias:
        - responda somente em JSON valido;
        - use exatamente as propriedades: summary, recommendation, priority, highlights;
        - priority deve ser exatamente um dos valores: Low, Medium, High, Critical;
        - highlights deve conter entre 1 e 4 itens;
        - escreva summary, recommendation e highlights em portugues do Brasil;
        - nao inclua markdown nem texto fora do JSON.
        """;

    public static IReadOnlyList<ChatMessage> CreateMessages(IReadOnlyCollection<MaintenanceAlertDto> alerts)
    {
        var payload = JsonSerializer.Serialize(alerts, JsonOptions);

        return
        [
            new ChatMessage(ChatRole.System, SystemPrompt),
            new ChatMessage(
                ChatRole.User,
                $"Analise os alertas de manutencao abaixo e responda apenas com JSON valido.\n\n{payload}")
        ];
    }
}

public sealed record MaintenanceAlertAgentResponse(
    string Summary,
    string Recommendation,
    MaintenancePriority Priority,
    IReadOnlyList<string> Highlights);
