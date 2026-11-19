using CarInvoice.Core.DTOs;
using CarInvoice.Core.Entities;
using CarInvoice.Core.Enums;

namespace CarInvoice.Core.Interfaces;

/// <summary>Parses messy JDM chassis strings and suggests make/model defaults.</summary>
public interface IChassisParserService
{
    Task<ChassisParseResult> ParseAsync(string rawInput, CancellationToken ct = default);
    Task<ChassisLookupResult> ResolveLookupsAsync(string chassisCode, string? chassisNumber = null, CancellationToken ct = default);
    string Normalize(string chassisCode, string chassisNumber);
}

/// <summary>Auto-learns chassis lookups and glossary terms from completed invoices.</summary>
public interface IDictionaryLearningService
{
    Task<DictionaryLearningResult> LearnFromInvoiceAsync(
        string chassisCode,
        string make,
        string model,
        int? engineCc,
        string? bodyType,
        int? doors,
        int? seats,
        string? fuelType,
        string? engineNumber,
        string? chassisNumber,
        string? exteriorColor,
        string? historyNotes,
        string? rawSourceText,
        CancellationToken ct = default);
}

/// <summary>Result summary for dictionary self-learning.</summary>
public class DictionaryLearningResult
{
    public bool ChassisLearned { get; set; }
    public string? ChassisMessage { get; set; }
    public int GlossaryTermsAdded { get; set; }
    public string? GlossaryMessage { get; set; }

    public string? StatusMessage
    {
        get
        {
            var parts = new List<string>();
            if (!string.IsNullOrWhiteSpace(ChassisMessage)) parts.Add(ChassisMessage);
            if (!string.IsNullOrWhiteSpace(GlossaryMessage)) parts.Add(GlossaryMessage);
            return parts.Count == 0 ? null : string.Join(" · ", parts);
        }
    }
}

/// <summary>Offline Japanese → English automotive glossary translator.</summary>
public interface IJapaneseTranslationService
{
    Task ReloadAsync(CancellationToken ct = default);
    Task<string> TranslateAsync(string text, bool keepOriginalInParentheses = true, CancellationToken ct = default);
    Task AddOrUpdateTermAsync(string japanese, string english, string category, CancellationToken ct = default);
}

/// <summary>Ingests vehicle data from live Outlook COM or offline .msg/.eml / clipboard text.</summary>
public interface IOutlookIngestionService
{
    /// <summary>True when Classic Outlook COM is ready (same as <see cref="CheckOutlookAvailability"/> == ClassicOutlookReady).</summary>
    bool IsOutlookAvailable();

    /// <summary>
    /// Probes Classic Outlook via ProgID / COM without throwing unhandled COM exceptions.
    /// New Outlook (WebView2) and missing Outlook map to <see cref="OutlookAvailabilityStatus.NewOutlookOrUnavailable"/>.
    /// </summary>
    OutlookAvailabilityStatus CheckOutlookAvailability();

    /// <summary>Friendly message when inbox sync is unavailable.</summary>
    string GetUnavailableMessage();

    Task<List<EmailIngestionResult>> FetchFromOutlookAsync(string? folderName = null, int maxCount = 20, bool unreadOnly = true, CancellationToken ct = default);
    Task<EmailIngestionResult> ParseMessageFileAsync(string filePath, CancellationToken ct = default);

    /// <summary>Parse pasted email / auction text (New Outlook Ctrl+A / Ctrl+C fallback).</summary>
    Task<EmailIngestionResult> ParsePlainTextAsync(string text, CancellationToken ct = default);

    ExtractedVehicleFields ExtractFields(string text);
}

/// <summary>Excel import/export for stock sheets, invoice ledgers, and dictionary tables.</summary>
public interface IExcelService
{
    Task<List<ExtractedVehicleFields>> ImportVehiclesAsync(string filePath, CancellationToken ct = default);
    Task ExportInvoicesAsync(IEnumerable<Invoice> invoices, string filePath, CancellationToken ct = default);
    Task ExportVehiclesAsync(IEnumerable<Vehicle> vehicles, string filePath, CancellationToken ct = default);

    Task ExportChassisLookupsAsync(IEnumerable<ChassisCodeLookup> rows, string filePath, CancellationToken ct = default);
    Task<DictionaryImportResult> ImportChassisLookupsAsync(string filePath, IChassisLookupRepository repo, CancellationToken ct = default);
    Task ExportGlossaryAsync(IEnumerable<JapaneseTermGlossary> rows, string filePath, CancellationToken ct = default);
    Task<DictionaryImportResult> ImportGlossaryAsync(string filePath, IGlossaryRepository repo, CancellationToken ct = default);
    Task ExportBankProfilesAsync(IEnumerable<BankProfile> rows, string filePath, CancellationToken ct = default);
    Task<DictionaryImportResult> ImportBankProfilesAsync(string filePath, IBankProfileRepository repo, CancellationToken ct = default);
    void WriteChassisLookupSampleTemplate(string filePath);
}

/// <summary>Summary for dictionary Excel upserts.</summary>
public class DictionaryImportResult
{
    public int Inserted { get; set; }
    public int Updated { get; set; }
    public int Skipped { get; set; }
    public string Summary => $"Imported {Inserted} new, updated {Updated} existing"
                             + (Skipped > 0 ? $", skipped {Skipped}" : string.Empty);
}

/// <summary>Generates Word .docx invoices from a MiniWord template.</summary>
public interface IWordInvoiceGeneratorService
{
    string EnsureDefaultTemplate(string templatesDirectory);
    Task<string> GenerateAsync(InvoiceGenerationRequest request, string templatePath, string outputDirectory, bool openAfter = false, CancellationToken ct = default);
}

/// <summary>Application path / preference settings.</summary>
public interface IAppSettingsService
{
    string DatabasePath { get; }
    string TemplatePath { get; set; }
    string OutputFolder { get; set; }
    string BackupFolder { get; set; }
    string Currency { get; set; }
    Task LoadAsync(CancellationToken ct = default);
    Task SaveAsync(CancellationToken ct = default);
}
