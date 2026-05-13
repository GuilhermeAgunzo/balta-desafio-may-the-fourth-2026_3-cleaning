using Cleaning.Core.DTOs;

namespace Cleaning.Core.Contracts;

public interface IMaintenanceAlertAnalyzer
{
    bool IsConfigured { get; }

    Task<AiMaintenanceAnalysisResult?> AnalyzeAsync(
        IReadOnlyCollection<MaintenanceAlertDto> alerts,
        CancellationToken cancellationToken);
}
