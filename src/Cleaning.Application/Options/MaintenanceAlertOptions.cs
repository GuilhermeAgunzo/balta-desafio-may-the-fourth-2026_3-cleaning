namespace Cleaning.Application.Options;

public sealed class MaintenanceAlertOptions
{
    public const string SectionName = "MaintenanceAlerts";

    public int LeadWindowInDays { get; init; } = 3;
}
