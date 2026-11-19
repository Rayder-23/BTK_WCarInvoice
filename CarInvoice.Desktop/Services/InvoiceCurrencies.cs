namespace CarInvoice.Desktop.Services;

/// <summary>
/// Common invoice currency codes. No FX conversion — the amount is whatever the user enters.
/// </summary>
public static class InvoiceCurrencies
{
    public static readonly string[] Common =
    [
        "USD", "PKR", "JPY", "EUR", "GBP", "AED", "SAR", "CNY", "AUD", "CAD", "CHF", "SGD"
    ];
}
