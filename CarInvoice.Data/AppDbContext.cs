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
    public DbSet<BankProfile> BankProfiles => Set<BankProfile>();
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
                SerialPrefixOrRange = x.SerialPrefixOrRange,
                Notes = x.Notes,
                DefaultEngineCc = x.DefaultEngineCc,
                DefaultFuelType = x.DefaultFuelType,
                DefaultBodyType = x.DefaultBodyType,
                DefaultDoors = x.DefaultDoors,
                DefaultSeats = x.DefaultSeats,
                DefaultEngineNumber = x.DefaultEngineNumber
            }).ToArray());

        modelBuilder.Entity<JapaneseTermGlossary>().HasData(
            SeedData.GlossaryTerms.Select((x, i) => new JapaneseTermGlossary
            {
                Id = i + 1,
                JapaneseTerm = x.JapaneseTerm,
                EnglishTranslation = x.EnglishTranslation,
                Category = x.Category
            }).ToArray());

        modelBuilder.Entity<BankProfile>().HasData(
            SeedData.BankProfiles.Select((x, i) => new BankProfile
            {
                Id = i + 1,
                ProfileName = x.ProfileName,
                IsDefault = x.IsDefault,
                BankName = x.BankName,
                AccountHolderName = x.AccountHolderName,
                AccountNumber = x.AccountNumber,
                BranchName = x.BranchName,
                BranchCode = x.BranchCode,
                BranchAddress = x.BranchAddress,
                SwiftCode = x.SwiftCode,
                IntermediaryBankName = x.IntermediaryBankName,
                IntermediaryCountry = x.IntermediaryCountry,
                IntermediaryBranch = x.IntermediaryBranch,
                IntermediarySwiftCode = x.IntermediarySwiftCode
            }).ToArray());

        base.OnModelCreating(modelBuilder);
    }
}
