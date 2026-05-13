using Cleaning.Core.DTOs;

namespace Cleaning.Core.Contracts;

public interface IMaintenanceAnalysisService
{
    Task<MaintenanceAnalysisDto> AnalyzeAsync(CancellationToken cancellationToken);
}
