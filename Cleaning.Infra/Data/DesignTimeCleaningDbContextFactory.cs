using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Cleaning.Infra.Data;

public sealed class DesignTimeCleaningDbContextFactory : IDesignTimeDbContextFactory<CleaningDbContext>
{
    public CleaningDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<CleaningDbContext>();
        optionsBuilder.UseSqlite("Data Source=cleaning.db");

        return new CleaningDbContext(optionsBuilder.Options);
    }
}
