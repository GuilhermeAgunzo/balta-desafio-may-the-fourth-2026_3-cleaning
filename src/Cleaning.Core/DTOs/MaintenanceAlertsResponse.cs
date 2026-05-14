namespace Cleaning.Core.DTOs;

public sealed record MaintenanceAlertsResponse(
    DateTimeOffset GeneratedAt,
    int LeadWindowInDays,
    IReadOnlyList<MaintenanceAlertDto> Items);
