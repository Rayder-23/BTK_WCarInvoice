using CarInvoice.Core.Seed;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CarInvoice.Data;

/// <summary>
/// Ensures the local SQLite database exists, applies schema, seeds lookups,
/// and optionally creates a dated backup copy.
/// </summary>
public static class DatabaseInitializer
{
    public static string GetDefaultDatabasePath()
    {
        var dir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "CarInvoice");
        Directory.CreateDirectory(dir);
        return Path.Combine(dir, "carinvoice.db");
    }

    public static string GetDefaultBackupFolder()
    {
        var dir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "CarInvoice", "Backups");
        Directory.CreateDirectory(dir);
        return dir;
    }

    /// <summary>
    /// Creates/migrates the database and seeds chassis + glossary data if empty.
    /// </summary>
    public static async Task InitializeAsync(IServiceProvider services, bool createBackup = true, CancellationToken ct = default)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var dbPath = db.Database.GetDbConnection().DataSource;
        if (createBackup && File.Exists(dbPath))
            BackupDatabase(dbPath);

        // Prefer EF migrations when available; fall back to EnsureCreated for fresh installs
        // that somehow lack a migrations assembly entry.
        var pending = await db.Database.GetPendingMigrationsAsync(ct);
        if (pending.Any() || !(await db.Database.GetAppliedMigrationsAsync(ct)).Any())
        {
            try
            {
                await db.Database.MigrateAsync(ct);
            }
            catch
            {
                await db.Database.EnsureCreatedAsync(ct);
            }
        }

        // Patch missing seed rows for databases created before HasData / migrations.
        if (!await db.ChassisCodeLookups.AnyAsync(ct))
        {
            await db.ChassisCodeLookups.AddRangeAsync(SeedData.ChassisLookups, ct);
            await db.SaveChangesAsync(ct);
        }

        if (!await db.JapaneseTermGlossary.AnyAsync(ct))
        {
            await db.JapaneseTermGlossary.AddRangeAsync(SeedData.GlossaryTerms, ct);
            await db.SaveChangesAsync(ct);
        }
    }

    public static void BackupDatabase(string databasePath, string? backupFolder = null)
    {
        if (!File.Exists(databasePath)) return;
        backupFolder ??= GetDefaultBackupFolder();
        Directory.CreateDirectory(backupFolder);
        var dest = Path.Combine(backupFolder, $"carinvoice_{DateTime.Now:yyyyMMdd_HHmmss}.db");
        File.Copy(databasePath, dest, overwrite: false);

        // Keep last 20 backups
        var old = Directory.GetFiles(backupFolder, "carinvoice_*.db")
            .OrderByDescending(f => f)
            .Skip(20);
        foreach (var f in old)
        {
            try { File.Delete(f); } catch { /* ignore */ }
        }
    }

    public static IServiceCollection AddCarInvoiceData(this IServiceCollection services, string? databasePath = null)
    {
        databasePath ??= GetDefaultDatabasePath();
        Directory.CreateDirectory(Path.GetDirectoryName(databasePath)!);

        // Singleton DbContext is appropriate for a single-user offline desktop app.
        services.AddDbContext<AppDbContext>(
            opt => opt.UseSqlite($"Data Source={databasePath}"),
            contextLifetime: ServiceLifetime.Singleton,
            optionsLifetime: ServiceLifetime.Singleton);

        services.AddSingleton<Core.Interfaces.IUnitOfWork, Repositories.UnitOfWork>();
        services.AddSingleton<Core.Interfaces.IVehicleRepository, Repositories.VehicleRepository>();
        services.AddSingleton<Core.Interfaces.ICustomerRepository, Repositories.CustomerRepository>();
        services.AddSingleton<Core.Interfaces.IInvoiceRepository, Repositories.InvoiceRepository>();
        services.AddSingleton<Core.Interfaces.IChassisLookupRepository, Repositories.ChassisLookupRepository>();
        services.AddSingleton<Core.Interfaces.IGlossaryRepository, Repositories.GlossaryRepository>();

        return services;
    }
}
