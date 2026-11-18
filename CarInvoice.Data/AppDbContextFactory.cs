using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CarInvoice.Data;

/// <summary>
/// Design-time factory used by <c>dotnet ef migrations</c>.
/// </summary>
public sealed class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite($"Data Source={DatabaseInitializer.GetDefaultDatabasePath()}")
            .Options;
        return new AppDbContext(options);
    }
}
