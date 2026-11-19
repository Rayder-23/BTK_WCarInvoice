using CarInvoice.Core.Entities;

namespace CarInvoice.Core.DTOs;

/// <summary>
/// Multi-model chassis code resolution result.
/// </summary>
public class ChassisLookupResult
{
    public List<ChassisCodeLookup> Matches { get; set; } = new();
    public bool RequiresModelSelection => Matches.Count > 1;
    public ChassisCodeLookup? RecommendedMatch { get; set; }
    public ChassisCodeLookup? SingleMatch => Matches.Count == 1 ? Matches[0] : null;
}

/// <summary>
/// UI-friendly option wrapping a lookup row for the multi-model picker.
/// </summary>
public class ChassisModelOption
{
    public ChassisCodeLookup Lookup { get; set; } = null!;
    public string DisplayLabel { get; set; } = string.Empty;
    public bool IsRecommended { get; set; }
    public string? RecommendationBadge => IsRecommended ? "Recommended Match" : null;
}
