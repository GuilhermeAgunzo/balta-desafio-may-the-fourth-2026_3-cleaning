using Cleaning.Core.Contracts;
using Cleaning.Core.DTOs;
using Cleaning.Core.Entities;

namespace Cleaning.Application.Tests;

internal sealed class InMemoryMaintenanceTaskRepository : IMaintenanceTaskRepository
{
    private readonly List<MaintenanceTask> _tasks = [];

    public Task<IReadOnlyList<MaintenanceTask>> ListAsync(CancellationToken cancellationToken)
        => Task.FromResult<IReadOnlyList<MaintenanceTask>>(_tasks.ToArray());

    public Task<MaintenanceTask?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => Task.FromResult(_tasks.FirstOrDefault(task => task.Id == id));

    public Task AddAsync(MaintenanceTask task, CancellationToken cancellationToken)
    {
        _tasks.Add(task);
        return Task.CompletedTask;
    }

    public void Remove(MaintenanceTask task) => _tasks.Remove(task);

    public Task SaveChangesAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}

internal sealed class FixedTimeProvider(DateTimeOffset current) : TimeProvider
{
    public DateTimeOffset Current { get; private set; } = current;

    public override DateTimeOffset GetUtcNow() => Current;

    public void Set(DateTimeOffset value) => Current = value;
}

internal sealed class StubAlertService(MaintenanceAlertsResponse response) : IMaintenanceAlertService
{
    public Task<MaintenanceAlertsResponse> GetAlertsAsync(CancellationToken cancellationToken)
        => Task.FromResult(response);
}

internal sealed class StubAlertAnalyzer(bool isConfigured, AiMaintenanceAnalysisResult? result) : IMaintenanceAlertAnalyzer
{
    public bool IsConfigured { get; } = isConfigured;

    public int CallCount { get; private set; }

    public IReadOnlyCollection<MaintenanceAlertDto>? CapturedAlerts { get; private set; }

    public Task<AiMaintenanceAnalysisResult?> AnalyzeAsync(
        IReadOnlyCollection<MaintenanceAlertDto> alerts,
        CancellationToken cancellationToken)
    {
        CallCount++;
        CapturedAlerts = alerts.ToArray();
        return Task.FromResult(result);
    }
}
