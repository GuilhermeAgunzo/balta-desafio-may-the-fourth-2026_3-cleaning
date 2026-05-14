using Cleaning.Core.Entities;

namespace Cleaning.Core.Contracts;

public interface IMaintenanceTaskRepository
{
    Task<IReadOnlyList<MaintenanceTask>> ListAsync(CancellationToken cancellationToken);

    Task<MaintenanceTask?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task AddAsync(MaintenanceTask task, CancellationToken cancellationToken);

    void Remove(MaintenanceTask task);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
