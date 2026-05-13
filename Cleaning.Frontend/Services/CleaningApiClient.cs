using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Cleaning.Core.DTOs;
using Cleaning.Core.Enums;

namespace Cleaning.Frontend.Services;

public sealed class CleaningApiClient(HttpClient httpClient)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    public async Task<IReadOnlyList<MaintenanceTaskDto>> GetTasksAsync(MaintenanceTaskFilter filter, CancellationToken cancellationToken = default)
        => await httpClient.GetFromJsonAsync<IReadOnlyList<MaintenanceTaskDto>>(
            $"/api/tasks?filter={filter.ToString().ToLowerInvariant()}",
            JsonOptions,
            cancellationToken)
        ?? [];

    public async Task<MaintenanceTaskDto> CreateTaskAsync(CreateMaintenanceTaskRequest request, CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.PostAsJsonAsync("/api/tasks", request, JsonOptions, cancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<MaintenanceTaskDto>(JsonOptions, cancellationToken)
            ?? throw new InvalidOperationException("A API nao retornou a tarefa criada.");
    }

    public async Task CompleteTaskAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.PostAsync($"/api/tasks/{id}/complete", null, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteTaskAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.DeleteAsync($"/api/tasks/{id}", cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task<MaintenanceAlertsResponse> GetAlertsAsync(CancellationToken cancellationToken = default)
        => await httpClient.GetFromJsonAsync<MaintenanceAlertsResponse>("/api/alerts", JsonOptions, cancellationToken)
        ?? new MaintenanceAlertsResponse(DateTimeOffset.UtcNow, 0, []);

    public async Task<MaintenanceAnalysisDto> GetAnalysisAsync(CancellationToken cancellationToken = default)
        => await httpClient.GetFromJsonAsync<MaintenanceAnalysisDto>("/api/analysis", JsonOptions, cancellationToken)
        ?? throw new InvalidOperationException("A API nao retornou a analise de manutencao.");
}
