using Cleaning.Core.Enums;

namespace Cleaning.Core.DTOs;

public sealed record MaintenanceAlertDto(
    Guid TaskId,
    string TaskName,
    string Description,
    DateOnly NextExecutionDate,
    int DaysUntilDue,
    MaintenanceTaskStatus Status,
    MaintenancePriority Priority,
    string Message);
