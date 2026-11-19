namespace CarInvoice.Core.Entities;

/// <summary>
/// Export / sales invoice linking a customer (and optional consignee) to a vehicle.
/// Aligns with MDK CORPORATION-style Japanese vehicle export invoices.
/// </summary>
public class Invoice
{
    public int Id { get; set; }

    /// <summary>Unique invoice number, e.g. 234223 or INV-2026-0001.</summary>
    public string InvoiceNumber { get; set; } = string.Empty;

    public DateTime InvoiceDate { get; set; } = DateTime.Today;
    public int CustomerId { get; set; }
    public int VehicleId { get; set; }

    public string? DestinationPort { get; set; }
    public string? DestinationCountry { get; set; }

    /// <summary>When true, Consignee Name/Address mirror the customer.</summary>
    public bool IsConsigneeSameAsCustomer { get; set; } = true;
    public string? ConsigneeName { get; set; }
    public string? ConsigneeAddress { get; set; }

    /// <summary>Trade term printed on invoice, e.g. C &amp; F, FOB, CIF.</summary>
    public string TradeTerm { get; set; } = "C & F";

    /// <summary>Unit / C&amp;F price amount (same currency as Currency).</summary>
    public decimal PriceAmount { get; set; }

    /// <summary>Legacy alias kept in sync with PriceAmount for older exports/UI.</summary>
    public decimal SalePrice { get; set; }

    public decimal TaxAmount { get; set; }
    public decimal AdditionalFees { get; set; }
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = "USD";
    public string PaymentMethod { get; set; } = "Cash";
    public string? Notes { get; set; }
    public string? GeneratedDocPath { get; set; }
    public bool IsDraft { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int? BankProfileId { get; set; }

    public Customer? Customer { get; set; }
    public Vehicle? Vehicle { get; set; }
    public BankProfile? BankProfile { get; set; }
}
