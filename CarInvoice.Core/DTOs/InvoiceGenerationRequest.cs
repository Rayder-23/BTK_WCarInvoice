namespace CarInvoice.Core.DTOs;

/// <summary>
/// Payload used to fill Word invoice template MiniWord {{Tag}} placeholders.
/// Tags match MDK-style export invoice layout.
/// </summary>
public class InvoiceGenerationRequest
{
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateTime InvoiceDate { get; set; } = DateTime.Today;

    public string? DestinationPort { get; set; }
    public string? DestinationCountry { get; set; }

    public string CustomerName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string CustomerAddress { get; set; } = string.Empty;
    public string? CompanyName { get; set; }

    public string ConsigneeName { get; set; } = string.Empty;
    public string ConsigneeAddress { get; set; } = string.Empty;

    public string? StockNumber { get; set; }
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
    public string MileageUnit { get; set; } = "KM";
    public int? Doors { get; set; }
    public string Condition { get; set; } = "USED";
    public string Steering { get; set; } = "RHD";
    public int? Seats { get; set; }
    public string? BodyType { get; set; }
    public string FuelType { get; set; } = string.Empty;
    public int? UnladenWeightKg { get; set; }
    public string Transmission { get; set; } = string.Empty;
    public string? AuctionGrade { get; set; }
    public string? HistoryNotes { get; set; }

    public string TradeTerm { get; set; } = "C & F";
    public decimal PriceAmount { get; set; }
    public decimal SalePrice { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal AdditionalFees { get; set; }
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = "USD";
    public string PaymentMethod { get; set; } = "Cash";
    public string? Notes { get; set; }

    public string? BankName { get; set; }
    public string? AccountHolderName { get; set; }
    public string? AccountNumber { get; set; }
    public string? BranchName { get; set; }
    public string? BranchCode { get; set; }
    public string? BranchAddress { get; set; }
    public string? SwiftCode { get; set; }
    public string? IntermediaryBankName { get; set; }
    public string? IntermediaryCountry { get; set; }
    public string? IntermediaryBranch { get; set; }
    public string? IntermediarySwiftCode { get; set; }
}
