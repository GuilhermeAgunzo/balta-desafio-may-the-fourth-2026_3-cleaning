using Cleaning.Core.Enums;

namespace Cleaning.Core.Entities;

public sealed class MaintenanceTask
{
    private MaintenanceTask()
    {
    }

    public MaintenanceTask(
        string name,
        string description,
        int recurrenceInterval,
        RecurrenceUnit recurrenceUnit,
        DateOnly firstExecutionDate,
        DateTimeOffset createdAt)
    {
        Id = Guid.NewGuid();
        Name = Normalize(name, 80, nameof(name));
        Description = Normalize(description, 400, nameof(description));
        RecurrenceInterval = ValidateInterval(recurrenceInterval);
        RecurrenceUnit = recurrenceUnit;
        NextExecutionDate = firstExecutionDate;
        CreatedAt = createdAt.UtcDateTime;
        UpdatedAt = createdAt.UtcDateTime;
    }

    public Guid Id { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public string Description { get; private set; } = string.Empty;

    public int RecurrenceInterval { get; private set; }

    public RecurrenceUnit RecurrenceUnit { get; private set; }

    public DateOnly NextExecutionDate { get; private set; }

    public DateOnly? LastCompletedOn { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime UpdatedAt { get; private set; }

    public void Complete(DateOnly completedOn, DateTimeOffset updatedAt)
    {
        LastCompletedOn = completedOn;
        NextExecutionDate = CalculateNextExecutionDate(completedOn);
        UpdatedAt = updatedAt.UtcDateTime;
    }

    public MaintenanceTaskStatus GetStatus(DateOnly referenceDate)
        => NextExecutionDate < referenceDate
            ? MaintenanceTaskStatus.Overdue
            : MaintenanceTaskStatus.Pending;

    public int GetDaysUntilDue(DateOnly referenceDate) => NextExecutionDate.DayNumber - referenceDate.DayNumber;

    public string GetRecurrenceLabel()
    {
        var label = RecurrenceUnit switch
        {
            RecurrenceUnit.Days => RecurrenceInterval == 1 ? "dia" : "dias",
            RecurrenceUnit.Weeks => RecurrenceInterval == 1 ? "semana" : "semanas",
            RecurrenceUnit.Months => RecurrenceInterval == 1 ? "mes" : "meses",
            _ => "ciclos"
        };

        return $"{RecurrenceInterval} {label}";
    }

    private DateOnly CalculateNextExecutionDate(DateOnly completedOn)
        => RecurrenceUnit switch
        {
            RecurrenceUnit.Days => completedOn.AddDays(RecurrenceInterval),
            RecurrenceUnit.Weeks => completedOn.AddDays(RecurrenceInterval * 7),
            RecurrenceUnit.Months => completedOn.AddMonths(RecurrenceInterval),
            _ => throw new InvalidOperationException($"Periodicidade desconhecida: {RecurrenceUnit}.")
        };

    private static int ValidateInterval(int recurrenceInterval)
        => recurrenceInterval < 1
            ? throw new ArgumentOutOfRangeException(nameof(recurrenceInterval), "A periodicidade deve ser maior que zero.")
            : recurrenceInterval;

    private static string Normalize(string value, int maxLength, string paramName)
    {
        var normalized = value.Trim();
        if (string.IsNullOrWhiteSpace(normalized))
        {
            throw new ArgumentException("O valor informado nao pode ser vazio.", paramName);
        }

        return normalized.Length <= maxLength
            ? normalized
            : throw new ArgumentException($"O valor informado excede o limite de {maxLength} caracteres.", paramName);
    }
}
