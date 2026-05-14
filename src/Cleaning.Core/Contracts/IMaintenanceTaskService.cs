using Cleaning.Core.DTOs;
using Cleaning.Core.Enums;

namespace Cleaning.Core.Contracts;

public interface IMaintenanceTaskService
{
    Task<MaintenanceTaskDto> CreateAsync(CreateMaintenanceTaskRequest request, CancellationToken cancellationToken);

    Task<IReadOnlyList<MaintenanceTaskDto>> ListAsync(MaintenanceTaskFilter filter, CancellationToken cancellationToken);

    Task<MaintenanceTaskDto?> CompleteAsync(Guid id, CancellationToken cancellationToken);

    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken);
}
