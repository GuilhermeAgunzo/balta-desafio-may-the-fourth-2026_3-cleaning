using Cleaning.Core.Enums;

namespace Cleaning.Core.DTOs;

public sealed record MaintenanceTaskDto(
    Guid Id,
    string Name,
    string Description,
    int RecurrenceInterval,
    RecurrenceUnit RecurrenceUnit,
    string RecurrenceLabel,
    DateOnly NextExecutionDate,
    DateOnly? LastCompletedOn,
    MaintenanceTaskStatus Status,
    int DaysUntilDue);
