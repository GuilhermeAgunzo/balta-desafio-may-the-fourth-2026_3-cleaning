using Cleaning.Core.Contracts;
using Cleaning.Core.DTOs;

namespace Cleaning.Infra.AI;

internal sealed class DisabledMaintenanceAlertAnalyzer : IMaintenanceAlertAnalyzer
{
    public bool IsConfigured => false;

    public Task<AiMaintenanceAnalysisResult?> AnalyzeAsync(
        IReadOnlyCollection<MaintenanceAlertDto> alerts,
        CancellationToken cancellationToken)
        => Task.FromResult<AiMaintenanceAnalysisResult?>(null);
}
