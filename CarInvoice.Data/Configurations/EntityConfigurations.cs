using CarInvoice.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CarInvoice.Data.Configurations;

public class VehicleConfiguration : IEntityTypeConfiguration<Vehicle>
{
    public void Configure(EntityTypeBuilder<Vehicle> builder)
    {
        builder.ToTable("Vehicles");
        builder.HasKey(x => x.Id);
        builder.Ignore(x => x.MakeModel);
        builder.Property(x => x.StockNumber).HasMaxLength(32);
        builder.Property(x => x.ChassisCode).HasMaxLength(32).IsRequired();
        builder.Property(x => x.ChassisNumber).HasMaxLength(32).IsRequired();
        builder.Property(x => x.FullChassisNormalized).HasMaxLength(64).IsRequired();
        builder.Property(x => x.Make).HasMaxLength(64).IsRequired();
        builder.Property(x => x.Model).HasMaxLength(128).IsRequired();
        builder.Property(x => x.GradeTrim).HasMaxLength(64);
        builder.Property(x => x.EngineNumber).HasMaxLength(64);
        builder.Property(x => x.ExteriorColor).HasMaxLength(64);
        builder.Property(x => x.FuelType).HasMaxLength(32).IsRequired().HasDefaultValue("PETROL");
        builder.Property(x => x.Transmission).HasMaxLength(32).IsRequired();
        builder.Property(x => x.MileageUnit).HasMaxLength(16).HasDefaultValue("KM");
        builder.Property(x => x.Condition).HasMaxLength(32).IsRequired().HasDefaultValue("USED");
        builder.Property(x => x.Steering).HasMaxLength(8).IsRequired().HasDefaultValue("RHD");
        builder.Property(x => x.BodyType).HasMaxLength(64);
        builder.Property(x => x.AuctionGrade).HasMaxLength(16);
        builder.Property(x => x.HistoryNotes).HasMaxLength(4000);

        builder.HasIndex(x => x.StockNumber);
        builder.HasIndex(x => x.ChassisCode);
        builder.HasIndex(x => x.ChassisNumber).IsUnique();
        builder.HasIndex(x => x.FullChassisNormalized);
        builder.HasIndex(x => new { x.Make, x.Model });
    }
}

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("Customers");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.FullName).HasMaxLength(200).IsRequired();
        builder.Property(x => x.CompanyName).HasMaxLength(200);
        builder.Property(x => x.Phone).HasMaxLength(64);
        builder.Property(x => x.Email).HasMaxLength(200);
        builder.Property(x => x.Address).HasMaxLength(500);
        builder.Property(x => x.TaxOrNationalId).HasMaxLength(64);
        builder.HasIndex(x => x.FullName);
        builder.HasIndex(x => x.Phone);
    }
}

public class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        builder.ToTable("Invoices");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.InvoiceNumber).HasMaxLength(32).IsRequired();
        builder.Property(x => x.DestinationPort).HasMaxLength(128);
        builder.Property(x => x.DestinationCountry).HasMaxLength(128);
        builder.Property(x => x.ConsigneeName).HasMaxLength(200);
        builder.Property(x => x.ConsigneeAddress).HasMaxLength(500);
        builder.Property(x => x.TradeTerm).HasMaxLength(32).HasDefaultValue("C & F");
        builder.Property(x => x.PriceAmount).HasPrecision(18, 2);
        builder.Property(x => x.SalePrice).HasPrecision(18, 2);
        builder.Property(x => x.TaxAmount).HasPrecision(18, 2);
        builder.Property(x => x.AdditionalFees).HasPrecision(18, 2);
        builder.Property(x => x.TotalAmount).HasPrecision(18, 2);
        builder.Property(x => x.Currency).HasMaxLength(8).HasDefaultValue("USD");
        builder.Property(x => x.PaymentMethod).HasMaxLength(64);
        builder.Property(x => x.Notes).HasMaxLength(2000);
        builder.Property(x => x.GeneratedDocPath).HasMaxLength(1000);

        builder.HasIndex(x => x.InvoiceNumber).IsUnique();
        builder.HasIndex(x => x.InvoiceDate);

        builder.HasOne(x => x.Customer)
            .WithMany(c => c.Invoices)
            .HasForeignKey(x => x.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Vehicle)
            .WithMany(v => v.Invoices)
            .HasForeignKey(x => x.VehicleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.BankProfile)
            .WithMany(b => b.Invoices)
            .HasForeignKey(x => x.BankProfileId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

public class BankProfileConfiguration : IEntityTypeConfiguration<BankProfile>
{
    public void Configure(EntityTypeBuilder<BankProfile> builder)
    {
        builder.ToTable("BankProfiles");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ProfileName).HasMaxLength(128).IsRequired();
        builder.Property(x => x.BankName).HasMaxLength(128).IsRequired();
        builder.Property(x => x.AccountHolderName).HasMaxLength(200).IsRequired();
        builder.Property(x => x.AccountNumber).HasMaxLength(64).IsRequired();
        builder.Property(x => x.BranchName).HasMaxLength(128).IsRequired();
        builder.Property(x => x.BranchCode).HasMaxLength(32).IsRequired();
        builder.Property(x => x.BranchAddress).HasMaxLength(500).IsRequired();
        builder.Property(x => x.SwiftCode).HasMaxLength(32).IsRequired();
        builder.Property(x => x.IntermediaryBankName).HasMaxLength(128);
        builder.Property(x => x.IntermediaryCountry).HasMaxLength(128);
        builder.Property(x => x.IntermediaryBranch).HasMaxLength(128);
        builder.Property(x => x.IntermediarySwiftCode).HasMaxLength(32);
        builder.HasIndex(x => x.ProfileName);
        builder.HasIndex(x => x.IsDefault);
    }
}

public class ChassisCodeLookupConfiguration : IEntityTypeConfiguration<ChassisCodeLookup>
{
    public void Configure(EntityTypeBuilder<ChassisCodeLookup> builder)
    {
        builder.ToTable("ChassisCodeLookups");
        builder.HasKey(x => x.Id);
        builder.Ignore(x => x.DisplayLabel);
        builder.Property(x => x.ChassisCode).HasMaxLength(32).IsRequired();
        builder.Property(x => x.DefaultMake).HasMaxLength(64).IsRequired();
        builder.Property(x => x.DefaultModel).HasMaxLength(128).IsRequired();
        builder.Property(x => x.SerialPrefixOrRange).HasMaxLength(64);
        builder.Property(x => x.Notes).HasMaxLength(500);
        builder.Property(x => x.DefaultFuelType).HasMaxLength(32).IsRequired();
        builder.Property(x => x.DefaultBodyType).HasMaxLength(64);
        builder.Property(x => x.DefaultEngineNumber).HasMaxLength(64);
        builder.HasIndex(x => x.ChassisCode);
        builder.HasIndex(x => new { x.ChassisCode, x.DefaultMake, x.DefaultModel, x.SerialPrefixOrRange })
            .IsUnique();
    }
}

public class JapaneseTermGlossaryConfiguration : IEntityTypeConfiguration<JapaneseTermGlossary>
{
    public void Configure(EntityTypeBuilder<JapaneseTermGlossary> builder)
    {
        builder.ToTable("JapaneseTermGlossary");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.JapaneseTerm).HasMaxLength(128).IsRequired();
        builder.Property(x => x.EnglishTranslation).HasMaxLength(256).IsRequired();
        builder.Property(x => x.Category).HasMaxLength(64).IsRequired();
        builder.HasIndex(x => x.JapaneseTerm).IsUnique();
        builder.HasIndex(x => x.Category);
    }
}

public class AppSettingConfiguration : IEntityTypeConfiguration<AppSetting>
{
    public void Configure(EntityTypeBuilder<AppSetting> builder)
    {
        builder.ToTable("AppSettings");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Key).HasMaxLength(64).IsRequired();
        builder.Property(x => x.Value).HasMaxLength(2000).IsRequired();
        builder.HasIndex(x => x.Key).IsUnique();
    }
}
