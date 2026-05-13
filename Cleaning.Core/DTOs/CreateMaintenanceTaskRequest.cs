using System.ComponentModel.DataAnnotations;
using Cleaning.Core.Enums;

namespace Cleaning.Core.DTOs;

public sealed class CreateMaintenanceTaskRequest
{
    [Required]
    [StringLength(80, MinimumLength = 3)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(400, MinimumLength = 10)]
    public string Description { get; set; } = string.Empty;

    [Range(1, 3650)]
    public int RecurrenceInterval { get; set; } = 1;

    [EnumDataType(typeof(RecurrenceUnit))]
    public RecurrenceUnit RecurrenceUnit { get; set; } = RecurrenceUnit.Weeks;

    public DateOnly FirstExecutionDate { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);
}
