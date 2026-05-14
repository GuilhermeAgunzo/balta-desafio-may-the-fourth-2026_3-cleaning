using Cleaning.Core.Enums;

namespace Cleaning.Core.DTOs;

public sealed record AiMaintenanceAnalysisResult(
    string Summary,
    string Recommendation,
    MaintenancePriority Priority,
    IReadOnlyList<string> Highlights);
