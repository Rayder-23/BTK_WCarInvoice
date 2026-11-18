using CarInvoice.Core.Entities;
using CarInvoice.Core.Seed;
using Microsoft.EntityFrameworkCore;

namespace CarInvoice.Data;

/// <summary>
/// SQLite application database context for offline-first CarInvoice.
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Vehicle> Vehicles => Set<Vehicle>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<ChassisCodeLookup> ChassisCodeLookups => Set<ChassisCodeLookup>();
    public DbSet<JapaneseTermGlossary> JapaneseTermGlossary => Set<JapaneseTermGlossary>();
    public DbSet<AppSetting> AppSettings => Set<AppSetting>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        modelBuilder.Entity<ChassisCodeLookup>().HasData(
            SeedData.ChassisLookups.Select((x, i) => new ChassisCodeLookup
            {
                Id = i + 1,
                ChassisCode = x.ChassisCode,
                DefaultMake = x.DefaultMake,
                DefaultModel = x.DefaultModel,
                DefaultEngineCc = x.DefaultEngineCc,
                DefaultFuelType = x.DefaultFuelType
            }).ToArray());

        modelBuilder.Entity<JapaneseTermGlossary>().HasData(
            SeedData.GlossaryTerms.Select((x, i) => new JapaneseTermGlossary
            {
                Id = i + 1,
                JapaneseTerm = x.JapaneseTerm,
                EnglishTranslation = x.EnglishTranslation,
                Category = x.Category
            }).ToArray());

        base.OnModelCreating(modelBuilder);
    }
}
