using CarInvoice.Core.Entities;

namespace CarInvoice.Core.DTOs;

/// <summary>
/// Result of parsing a raw chassis string and looking up defaults / duplicates.
/// </summary>
public class ChassisParseResult
{
    public string ChassisCode { get; set; } = string.Empty;
    public string ChassisNumber { get; set; } = string.Empty;
    public string FullChassisNormalized { get; set; } = string.Empty;
    public bool IsVin { get; set; }

    public ChassisLookupResult Lookup { get; set; } = new();
    public bool RequiresModelSelection => Lookup.RequiresModelSelection;

    public string? SuggestedMake { get; set; }
    public string? SuggestedModel { get; set; }
    public int? SuggestedEngineCc { get; set; }
    public string? SuggestedFuelType { get; set; }
    public string? SuggestedBodyType { get; set; }
    public int? SuggestedDoors { get; set; }
    public int? SuggestedSeats { get; set; }
    public string? SuggestedEngineNumber { get; set; }

    public bool IsDuplicate { get; set; }
    public Vehicle? ExistingVehicle { get; set; }
    public string? DuplicateWarning { get; set; }
}
