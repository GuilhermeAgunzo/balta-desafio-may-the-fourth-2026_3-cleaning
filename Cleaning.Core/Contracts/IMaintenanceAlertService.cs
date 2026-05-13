using Cleaning.Core.DTOs;

namespace Cleaning.Core.Contracts;

public interface IMaintenanceAlertService
{
    Task<MaintenanceAlertsResponse> GetAlertsAsync(CancellationToken cancellationToken);
}
