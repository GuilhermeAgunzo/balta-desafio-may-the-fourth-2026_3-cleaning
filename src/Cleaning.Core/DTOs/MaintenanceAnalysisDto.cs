using Cleaning.Core.Enums;

namespace Cleaning.Core.DTOs;

public sealed record MaintenanceAnalysisDto(
    DateTimeOffset GeneratedAt,
    bool AiConfigured,
    bool UsedFallback,
    string Source,
    MaintenancePriority Priority,
    string Summary,
    string Recommendation,
    IReadOnlyList<string> Highlights);
