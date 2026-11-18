using CarInvoice.Core.DTOs;
using CarInvoice.Core.Entities;

namespace CarInvoice.Core.Interfaces;

/// <summary>Parses messy JDM chassis strings and suggests make/model defaults.</summary>
public interface IChassisParserService
{
    Task<ChassisParseResult> ParseAsync(string rawInput, CancellationToken ct = default);
    string Normalize(string chassisCode, string chassisNumber);
}

/// <summary>Offline Japanese → English automotive glossary translator.</summary>
public interface IJapaneseTranslationService
{
    Task ReloadAsync(CancellationToken ct = default);
    Task<string> TranslateAsync(string text, bool keepOriginalInParentheses = true, CancellationToken ct = default);
    Task AddOrUpdateTermAsync(string japanese, string english, string category, CancellationToken ct = default);
}

/// <summary>Ingests vehicle data from live Outlook COM or offline .msg/.eml files.</summary>
public interface IOutlookIngestionService
{
    bool IsOutlookAvailable();
    Task<List<EmailIngestionResult>> FetchFromOutlookAsync(string? folderName = null, int maxCount = 20, bool unreadOnly = true, CancellationToken ct = default);
    Task<EmailIngestionResult> ParseMessageFileAsync(string filePath, CancellationToken ct = default);
    ExtractedVehicleFields ExtractFields(string text);
}

/// <summary>Excel import/export for stock sheets and invoice ledgers.</summary>
public interface IExcelService
{
    Task<List<ExtractedVehicleFields>> ImportVehiclesAsync(string filePath, CancellationToken ct = default);
    Task ExportInvoicesAsync(IEnumerable<Invoice> invoices, string filePath, CancellationToken ct = default);
    Task ExportVehiclesAsync(IEnumerable<Vehicle> vehicles, string filePath, CancellationToken ct = default);
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
