using Cleaning.Core.Contracts;
using Cleaning.Core.Entities;
using Cleaning.Infra.Data;
using Microsoft.EntityFrameworkCore;

namespace Cleaning.Infra.Repositories;

internal sealed class MaintenanceTaskRepository(CleaningDbContext dbContext) : IMaintenanceTaskRepository
{
    public async Task<IReadOnlyList<MaintenanceTask>> ListAsync(CancellationToken cancellationToken)
        => await dbContext.MaintenanceTasks
            .OrderBy(task => task.NextExecutionDate)
            .ThenBy(task => task.Name)
            .ToListAsync(cancellationToken);

    public Task<MaintenanceTask?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => dbContext.MaintenanceTasks.FirstOrDefaultAsync(task => task.Id == id, cancellationToken);

    public Task AddAsync(MaintenanceTask task, CancellationToken cancellationToken)
        => dbContext.MaintenanceTasks.AddAsync(task, cancellationToken).AsTask();

    public void Remove(MaintenanceTask task) => dbContext.MaintenanceTasks.Remove(task);

    public Task SaveChangesAsync(CancellationToken cancellationToken) => dbContext.SaveChangesAsync(cancellationToken);
}
