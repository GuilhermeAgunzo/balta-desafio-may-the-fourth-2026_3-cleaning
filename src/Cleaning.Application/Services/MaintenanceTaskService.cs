using Cleaning.Core.Contracts;
using Cleaning.Core.DTOs;
using Cleaning.Core.Entities;
using Cleaning.Core.Enums;

namespace Cleaning.Application.Services;

internal sealed class MaintenanceTaskService(
    IMaintenanceTaskRepository repository,
    TimeProvider timeProvider) : IMaintenanceTaskService
{
    public async Task<MaintenanceTaskDto> CreateAsync(CreateMaintenanceTaskRequest request, CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow();
        var task = new MaintenanceTask(
            request.Name,
            request.Description,
            request.RecurrenceInterval,
            request.RecurrenceUnit,
            request.FirstExecutionDate,
            now);

        await repository.AddAsync(task, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        return MaintenanceMapping.ToTaskDto(task, GetReferenceDate());
    }

    public async Task<IReadOnlyList<MaintenanceTaskDto>> ListAsync(MaintenanceTaskFilter filter, CancellationToken cancellationToken)
    {
        var referenceDate = GetReferenceDate();
        var tasks = await repository.ListAsync(cancellationToken);

        return tasks
            .Where(task => filter switch
            {
                MaintenanceTaskFilter.Pending => task.GetStatus(referenceDate) == MaintenanceTaskStatus.Pending,
                MaintenanceTaskFilter.Overdue => task.GetStatus(referenceDate) == MaintenanceTaskStatus.Overdue,
                _ => true
            })
            .OrderBy(task => task.NextExecutionDate)
            .ThenBy(task => task.Name)
            .Select(task => MaintenanceMapping.ToTaskDto(task, referenceDate))
            .ToArray();
    }

    public async Task<MaintenanceTaskDto?> CompleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var task = await repository.GetByIdAsync(id, cancellationToken);
        if (task is null)
        {
            return null;
        }

        var now = timeProvider.GetUtcNow();
        var today = DateOnly.FromDateTime(now.UtcDateTime);

        task.Complete(today, now);
        await repository.SaveChangesAsync(cancellationToken);

        return MaintenanceMapping.ToTaskDto(task, GetReferenceDate());
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var task = await repository.GetByIdAsync(id, cancellationToken);
        if (task is null)
        {
            return false;
        }

        repository.Remove(task);
        await repository.SaveChangesAsync(cancellationToken);

        return true;
    }

    private DateOnly GetReferenceDate() => DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime);
}
