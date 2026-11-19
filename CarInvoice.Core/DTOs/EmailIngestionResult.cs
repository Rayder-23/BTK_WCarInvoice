namespace CarInvoice.Core.DTOs;

/// <summary>
/// Parsed email content plus smart-extracted vehicle/customer fields.
/// </summary>
public class EmailIngestionResult
{
    public string Subject { get; set; } = string.Empty;
    public string Sender { get; set; } = string.Empty;
    public string SenderEmail { get; set; } = string.Empty;
    public string BodyText { get; set; } = string.Empty;
    public string BodyHtml { get; set; } = string.Empty;
    public DateTime? ReceivedAt { get; set; }
    public List<string> AttachmentNames { get; set; } = new();
    public ExtractedVehicleFields Extracted { get; set; } = new();
}

/// <summary>
/// Fields extracted from email / Excel free text via regex patterns (MDK export style).
/// </summary>
public class ExtractedVehicleFields
{
    public string? StockNumber { get; set; }
    public string? ChassisCode { get; set; }
    public string? ChassisNumber { get; set; }
    public string? Make { get; set; }
    public string? Model { get; set; }
    public int? Year { get; set; }
    public int? Mileage { get; set; }
    public string? Color { get; set; }
    public decimal? Price { get; set; }
    public string? CustomerName { get; set; }
    public string? CustomerAddress { get; set; }
    public string? ConsigneeName { get; set; }
    public string? ConsigneeAddress { get; set; }
    public string? DestinationPort { get; set; }
    public string? DestinationCountry { get; set; }
    public string? TradeTerm { get; set; }
    public string? AuctionGrade { get; set; }
    public string? JapaneseRemarks { get; set; }
    public string? TranslatedRemarks { get; set; }
    public string? FuelType { get; set; }
    public string? Transmission { get; set; }
    public string? GradeTrim { get; set; }
    public int? EngineCc { get; set; }
    public string? EngineNumber { get; set; }
    public int? Doors { get; set; }
    public int? Seats { get; set; }
    public string? Condition { get; set; }
    public string? Steering { get; set; }
    public string? BodyType { get; set; }
    public int? UnladenWeightKg { get; set; }
}
