using Cleaning.Application.Options;
using Cleaning.Core.Contracts;
using Cleaning.Core.DTOs;
using Microsoft.Extensions.Options;

namespace Cleaning.Application.Services;

internal sealed class MaintenanceAlertService(
    IMaintenanceTaskRepository repository,
    IOptions<MaintenanceAlertOptions> options,
    TimeProvider timeProvider) : IMaintenanceAlertService
{
    private readonly MaintenanceAlertOptions _options = options.Value;

    public async Task<MaintenanceAlertsResponse> GetAlertsAsync(CancellationToken cancellationToken)
    {
        var referenceDate = DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime);
        var tasks = await repository.ListAsync(cancellationToken);

        var alerts = tasks
            .Select(task => MaintenanceMapping.ToAlertDto(task, referenceDate))
            .Where(alert => alert.DaysUntilDue <= _options.LeadWindowInDays)
            .OrderByDescending(alert => MaintenanceMapping.GetPriorityWeight(alert.Priority))
            .ThenBy(alert => alert.NextExecutionDate)
            .ThenBy(alert => alert.TaskName)
            .ToArray();

        return new MaintenanceAlertsResponse(
            timeProvider.GetUtcNow(),
            _options.LeadWindowInDays,
            alerts);
    }
}
