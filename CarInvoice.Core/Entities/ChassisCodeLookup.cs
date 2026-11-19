namespace CarInvoice.Core.Entities;

/// <summary>
/// Local reference row mapping a JDM chassis code to default make/model/engine/fuel/body.
/// The same chassis code may map to multiple models (e.g. KSP130 → Vitz and Probox).
/// </summary>
public class ChassisCodeLookup
{
    public int Id { get; set; }
    public string ChassisCode { get; set; } = string.Empty;
    public string DefaultMake { get; set; } = string.Empty;
    public string DefaultModel { get; set; } = string.Empty;

    /// <summary>
    /// Optional serial discriminator, e.g. "2000000-2999999" or "70xxxxx".
    /// </summary>
    public string? SerialPrefixOrRange { get; set; }

    public string? Notes { get; set; }
    public int? DefaultEngineCc { get; set; }
    public string DefaultFuelType { get; set; } = "PETROL";
    public string? DefaultBodyType { get; set; }
    public int? DefaultDoors { get; set; }
    public int? DefaultSeats { get; set; }
    public string? DefaultEngineNumber { get; set; }

    /// <summary>Display label for multi-model pickers.</summary>
    public string DisplayLabel
    {
        get
        {
            var parts = new List<string>();
            if (DefaultEngineCc is > 0) parts.Add($"{DefaultEngineCc}cc");
            if (!string.IsNullOrWhiteSpace(DefaultBodyType)) parts.Add(DefaultBodyType);
            var detail = parts.Count > 0 ? $" ({string.Join(" • ", parts)})" : string.Empty;
            return $"{DefaultMake} {DefaultModel}{detail}".Trim();
        }
    }
}
