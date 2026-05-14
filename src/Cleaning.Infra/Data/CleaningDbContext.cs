using Cleaning.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace Cleaning.Infra.Data;

public sealed class CleaningDbContext(DbContextOptions<CleaningDbContext> options) : DbContext(options)
{
    public DbSet<MaintenanceTask> MaintenanceTasks => Set<MaintenanceTask>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CleaningDbContext).Assembly);
    }
}
