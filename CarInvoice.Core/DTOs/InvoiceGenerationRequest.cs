namespace CarInvoice.Core.DTOs;

/// <summary>
/// Payload used to fill Word invoice template tags.
/// </summary>
public class InvoiceGenerationRequest
{
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateTime InvoiceDate { get; set; } = DateTime.Today;
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string CustomerAddress { get; set; } = string.Empty;
    public string? CompanyName { get; set; }
    public string Make { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string? GradeTrim { get; set; }
    public int Year { get; set; }
    public string ChassisCode { get; set; } = string.Empty;
    public string ChassisNumber { get; set; } = string.Empty;
    public int? EngineCc { get; set; }
    public string? EngineNumber { get; set; }
    public string? Color { get; set; }
    public int Mileage { get; set; }
    public string MileageUnit { get; set; } = "km";
    public string FuelType { get; set; } = string.Empty;
    public string Transmission { get; set; } = string.Empty;
    public string? AuctionGrade { get; set; }
    public string? HistoryNotes { get; set; }
    public decimal SalePrice { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal AdditionalFees { get; set; }
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = "PKR";
    public string PaymentMethod { get; set; } = "Cash";
    public string? Notes { get; set; }
}
