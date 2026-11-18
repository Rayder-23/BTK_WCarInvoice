namespace CarInvoice.Core.Entities;

/// <summary>
/// Local reference row mapping a JDM chassis code to default make/model/engine/fuel.
/// </summary>
public class ChassisCodeLookup
{
    public int Id { get; set; }
    public string ChassisCode { get; set; } = string.Empty;
    public string DefaultMake { get; set; } = string.Empty;
    public string DefaultModel { get; set; } = string.Empty;
    public int? DefaultEngineCc { get; set; }
    public string DefaultFuelType { get; set; } = "Petrol";
}
