using Cleaning.Core.DTOs;
using Cleaning.Core.Entities;
using Cleaning.Core.Enums;

namespace Cleaning.Application.Services;

internal static class MaintenanceMapping
{
    public static MaintenanceTaskDto ToTaskDto(MaintenanceTask task, DateOnly referenceDate)
        => new(
            task.Id,
            task.Name,
            task.Description,
            task.RecurrenceInterval,
            task.RecurrenceUnit,
            task.GetRecurrenceLabel(),
            task.NextExecutionDate,
            task.LastCompletedOn,
            task.GetStatus(referenceDate),
            task.GetDaysUntilDue(referenceDate));

    public static MaintenanceAlertDto ToAlertDto(MaintenanceTask task, DateOnly referenceDate)
    {
        var daysUntilDue = task.GetDaysUntilDue(referenceDate);
        var status = task.GetStatus(referenceDate);
        var priority = GetPriority(daysUntilDue);

        var message = daysUntilDue switch
        {
            < 0 => $"{task.Name} esta atrasada ha {Math.Abs(daysUntilDue)} dia(s).",
            0 => $"{task.Name} precisa ser executada hoje.",
            _ => $"{task.Name} vence em {daysUntilDue} dia(s)."
        };

        return new MaintenanceAlertDto(
            task.Id,
            task.Name,
            task.Description,
            task.NextExecutionDate,
            daysUntilDue,
            status,
            priority,
            message);
    }

    public static MaintenancePriority GetPriority(int daysUntilDue)
        => daysUntilDue switch
        {
            <= -7 => MaintenancePriority.Critical,
            < 0 => MaintenancePriority.High,
            0 => MaintenancePriority.High,
            <= 2 => MaintenancePriority.Medium,
            _ => MaintenancePriority.Low
        };

    public static int GetPriorityWeight(MaintenancePriority priority)
        => priority switch
        {
            MaintenancePriority.Critical => 3,
            MaintenancePriority.High => 2,
            MaintenancePriority.Medium => 1,
            _ => 0
        };
}
