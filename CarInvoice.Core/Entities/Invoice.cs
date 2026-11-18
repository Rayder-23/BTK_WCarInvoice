namespace CarInvoice.Core.Entities;

/// <summary>
/// Sales invoice linking a customer to a vehicle with pricing details.
/// </summary>
public class Invoice
{
    public int Id { get; set; }

    /// <summary>Formatted unique number, e.g. INV-2026-0001.</summary>
    public string InvoiceNumber { get; set; } = string.Empty;

    public DateTime InvoiceDate { get; set; } = DateTime.Today;
    public int CustomerId { get; set; }
    public int VehicleId { get; set; }
    public decimal SalePrice { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal AdditionalFees { get; set; }
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = "PKR";
    public string PaymentMethod { get; set; } = "Cash";
    public string? Notes { get; set; }
    public string? GeneratedDocPath { get; set; }
    public bool IsDraft { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Customer? Customer { get; set; }
    public Vehicle? Vehicle { get; set; }
}
