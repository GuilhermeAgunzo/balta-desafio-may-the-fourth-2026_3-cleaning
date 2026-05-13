using Cleaning.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cleaning.Infra.Data.Configurations;

internal sealed class MaintenanceTaskConfiguration : IEntityTypeConfiguration<MaintenanceTask>
{
    public void Configure(EntityTypeBuilder<MaintenanceTask> builder)
    {
        builder.ToTable("maintenance_tasks");

        builder.HasKey(task => task.Id);

        builder.Property(task => task.Name)
            .HasMaxLength(80)
            .IsRequired();

        builder.Property(task => task.Description)
            .HasMaxLength(400)
            .IsRequired();

        builder.Property(task => task.RecurrenceUnit)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(task => task.RecurrenceInterval)
            .IsRequired();

        builder.Property(task => task.NextExecutionDate)
            .IsRequired();

        builder.Property(task => task.CreatedAt)
            .IsRequired();

        builder.Property(task => task.UpdatedAt)
            .IsRequired();
    }
}
