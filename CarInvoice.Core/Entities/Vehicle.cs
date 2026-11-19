namespace CarInvoice.Core.Entities;

/// <summary>
/// Represents a second-hand / JDM vehicle in the local inventory registry.
/// Fields align with MDK-style Japanese export invoice specification grids.
/// </summary>
public class Vehicle
{
    public int Id { get; set; }

    /// <summary>Stock / inventory number from auction sheet, e.g. 112274.</summary>
    public string? StockNumber { get; set; }

    /// <summary>Chassis / model code, e.g. VR2E26, NZE161.</summary>
    public string ChassisCode { get; set; } = string.Empty;

    /// <summary>Serial portion, e.g. 123481 — printed as ChassisCode-ChassisNumber.</summary>
    public string ChassisNumber { get; set; } = string.Empty;

    /// <summary>Uppercase ChassisCode+ChassisNumber with spaces/hyphens stripped for instant lookup.</summary>
    public string FullChassisNormalized { get; set; } = string.Empty;

    public string Make { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;

    /// <summary>Combined maker line for invoices, e.g. NISSAN CARAVAN.</summary>
    public string MakeModel => $"{Make} {Model}".Trim();

    public string? GradeTrim { get; set; }
    public int ManufacturingYear { get; set; }
    public int? RegistrationYear { get; set; }
    public int? EngineCapacityCc { get; set; }
    public string? EngineNumber { get; set; }
    public string? ExteriorColor { get; set; }
    public string FuelType { get; set; } = "PETROL";
    public string Transmission { get; set; } = "Automatic";
    public int Mileage { get; set; }
    public string MileageUnit { get; set; } = "KM";
    public int? Doors { get; set; }
    public string Condition { get; set; } = "USED";
    public string Steering { get; set; } = "RHD";
    public int? Seats { get; set; }
    public string? BodyType { get; set; }
    public int? UnladenWeightKg { get; set; }
    public string? AuctionGrade { get; set; }
    public string? HistoryNotes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
}
