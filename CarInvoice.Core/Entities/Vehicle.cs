namespace CarInvoice.Core.Entities;

/// <summary>
/// Represents a second-hand / JDM vehicle in the local inventory registry.
/// </summary>
public class Vehicle
{
    public int Id { get; set; }

    /// <summary>Chassis / model code, e.g. NZE161, ZVW30, DBA-GRS204.</summary>
    public string ChassisCode { get; set; } = string.Empty;

    /// <summary>Serial / VIN portion, e.g. 7048291 or full 17-char VIN.</summary>
    public string ChassisNumber { get; set; } = string.Empty;

    /// <summary>Uppercase ChassisCode+ChassisNumber with spaces/hyphens stripped for instant lookup.</summary>
    public string FullChassisNormalized { get; set; } = string.Empty;

    public string Make { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string? GradeTrim { get; set; }
    public int ManufacturingYear { get; set; }
    public int? RegistrationYear { get; set; }
    public int? EngineCapacityCc { get; set; }
    public string? EngineNumber { get; set; }
    public string? ExteriorColor { get; set; }
    public string FuelType { get; set; } = "Petrol";
    public string Transmission { get; set; } = "Automatic";
    public int Mileage { get; set; }
    public string MileageUnit { get; set; } = "km";
    public string? AuctionGrade { get; set; }
    public string? HistoryNotes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
}
