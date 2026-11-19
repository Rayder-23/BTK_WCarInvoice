namespace CarInvoice.Core.Entities;

/// <summary>
/// Remittance / bank details printed on export invoices (e.g. MUFG Kanazawa).
/// </summary>
public class BankProfile
{
    public int Id { get; set; }
    public string ProfileName { get; set; } = string.Empty;
    public bool IsDefault { get; set; }

    public string BankName { get; set; } = string.Empty;
    public string AccountHolderName { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
    public string BranchName { get; set; } = string.Empty;
    public string BranchCode { get; set; } = string.Empty;
    public string BranchAddress { get; set; } = string.Empty;
    public string SwiftCode { get; set; } = string.Empty;

    public string? IntermediaryBankName { get; set; }
    public string? IntermediaryCountry { get; set; }
    public string? IntermediaryBranch { get; set; }
    public string? IntermediarySwiftCode { get; set; }

    public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
}
