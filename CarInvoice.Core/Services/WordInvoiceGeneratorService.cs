using System.Diagnostics;
using System.Globalization;
using System.IO.Compression;
using System.Text;
using CarInvoice.Core.DTOs;
using CarInvoice.Core.Interfaces;
using MiniSoftware;

namespace CarInvoice.Core.Services;

/// <summary>
/// Generates MDK-style export invoice .docx files from a MiniWord template,
/// creating a placeholder template on first run when none exists.
/// </summary>
public sealed class WordInvoiceGeneratorService : IWordInvoiceGeneratorService
{
    /// <inheritdoc />
    public string EnsureDefaultTemplate(string templatesDirectory)
    {
        Directory.CreateDirectory(templatesDirectory);
        var path = Path.Combine(templatesDirectory, "DefaultInvoiceTemplate.docx");
        if (!File.Exists(path) || !TemplateLooksCurrent(path))
            CreatePlaceholderTemplate(path);
        return path;
    }

    private static bool TemplateLooksCurrent(string path)
    {
        try
        {
            using var zip = ZipFile.OpenRead(path);
            var entry = zip.GetEntry("word/document.xml");
            if (entry is null) return false;
            using var stream = entry.Open();
            using var reader = new StreamReader(stream);
            var xml = reader.ReadToEnd();
            // Regenerate when legacy "TOTAL USD" / dollar-only formatting is still baked in
            return xml.Contains("{{InvoiceNo}}", StringComparison.Ordinal)
                   && xml.Contains("{{CandF}}", StringComparison.Ordinal)
                   && xml.Contains("{{ConsigneeName}}", StringComparison.Ordinal)
                   && xml.Contains("{{Currency}}", StringComparison.Ordinal)
                   && !xml.Contains("TOTAL USD", StringComparison.Ordinal);
        }
        catch
        {
            return false;
        }
    }

    /// <inheritdoc />
    public Task<string> GenerateAsync(
        InvoiceGenerationRequest request,
        string templatePath,
        string outputDirectory,
        bool openAfter = false,
        CancellationToken ct = default)
    {
        if (!File.Exists(templatePath))
            templatePath = EnsureDefaultTemplate(Path.GetDirectoryName(templatePath) ?? "Templates");

        Directory.CreateDirectory(outputDirectory);
        var chassisLabel = $"{request.ChassisCode}-{request.ChassisNumber}".Trim('-');
        var fileBase = string.IsNullOrWhiteSpace(chassisLabel) || chassisLabel == "-"
            ? string.Join("_", request.InvoiceNumber.Split(Path.GetInvalidFileNameChars()))
            : string.Join("_", $"{chassisLabel}-INV".Split(Path.GetInvalidFileNameChars()));
        var outputPath = Path.Combine(outputDirectory, $"{fileBase}.docx");
        if (File.Exists(outputPath))
            outputPath = Path.Combine(outputDirectory, $"{fileBase}_{DateTime.Now:yyyyMMdd_HHmmss}.docx");

        var price = request.PriceAmount > 0 ? request.PriceAmount : request.SalePrice;
        var total = request.TotalAmount > 0 ? request.TotalAmount : price;
        var currency = string.IsNullOrWhiteSpace(request.Currency)
            ? "USD"
            : request.Currency.Trim().ToUpperInvariant();
        var maker = $"{request.Make} {request.Model}".Trim();
        var odometer = $"{request.Mileage:N0} {request.MileageUnit}".Replace(",", "");
        var unladen = request.UnladenWeightKg is > 0
            ? $"{request.UnladenWeightKg.Value} KG"
            : string.Empty;

        var values = new Dictionary<string, object>
        {
            // Header & buyer
            ["InvoiceNo"] = request.InvoiceNumber,
            ["InvoiceNumber"] = request.InvoiceNumber,
            ["InvoiceDate"] = FormatInvoiceDate(request.InvoiceDate),
            ["Port"] = request.DestinationPort ?? string.Empty,
            ["Country"] = request.DestinationCountry ?? string.Empty,
            ["CustomerName"] = request.CustomerName,
            ["CustomerAddress"] = request.CustomerAddress,
            ["CustomerPhone"] = request.CustomerPhone,
            ["CustomerEmail"] = request.CustomerEmail,
            ["CompanyName"] = request.CompanyName ?? string.Empty,
            ["ConsigneeName"] = string.IsNullOrWhiteSpace(request.ConsigneeName) ? request.CustomerName : request.ConsigneeName,
            ["ConsigneeAddress"] = string.IsNullOrWhiteSpace(request.ConsigneeAddress) ? request.CustomerAddress : request.ConsigneeAddress,

            // Vehicle row 1
            ["StockNo"] = request.StockNumber ?? string.Empty,
            ["Maker"] = maker,
            ["Make"] = request.Make,
            ["Model"] = request.Model,
            ["Chassis"] = chassisLabel,
            ["ChassisCode"] = request.ChassisCode,
            ["ChassisNumber"] = request.ChassisNumber,
            ["CC"] = request.EngineCc?.ToString() ?? string.Empty,
            ["EngineCc"] = request.EngineCc?.ToString() ?? string.Empty,
            ["Color"] = request.Color ?? string.Empty,
            ["Year"] = request.Year.ToString(),
            ["Odometer"] = odometer,
            ["Mileage"] = odometer,
            ["Door"] = request.Doors?.ToString() ?? string.Empty,

            // Vehicle row 2
            ["Condition"] = request.Condition,
            ["Steering"] = request.Steering,
            ["EngineNo"] = request.EngineNumber ?? string.Empty,
            ["EngineNumber"] = request.EngineNumber ?? string.Empty,
            ["Seat"] = request.Seats?.ToString() ?? string.Empty,
            ["Body"] = request.BodyType ?? string.Empty,
            ["Fuel"] = request.FuelType,
            ["FuelType"] = request.FuelType,
            ["UnladenWeight"] = unladen,
            ["CandF"] = FormatMoney(price, currency),
            ["TradeTerm"] = request.TradeTerm,

            // Bank & totals (from selected BankProfile + AppSettings currency)
            ["BankName"] = request.BankName ?? string.Empty,
            ["AccountHolderName"] = request.AccountHolderName ?? string.Empty,
            ["AccountNo"] = request.AccountNumber ?? string.Empty,
            ["AccountNumber"] = request.AccountNumber ?? string.Empty,
            ["Branch"] = request.BranchName ?? string.Empty,
            ["BranchCode"] = request.BranchCode ?? string.Empty,
            ["BranchAddress"] = request.BranchAddress ?? string.Empty,
            ["SwiftCode"] = request.SwiftCode ?? string.Empty,
            ["IntermediaryBankName"] = request.IntermediaryBankName ?? string.Empty,
            ["IntermediaryCountry"] = request.IntermediaryCountry ?? string.Empty,
            ["IntermediaryBranch"] = request.IntermediaryBranch ?? string.Empty,
            ["IntermediarySwiftCode"] = request.IntermediarySwiftCode ?? string.Empty,
            // TotalUsd kept as alias for older custom templates
            ["TotalUsd"] = FormatMoney(total, currency),
            ["TotalAmount"] = FormatMoney(total, currency),
            ["SalePrice"] = FormatMoney(price, currency),
            ["TaxAmount"] = FormatMoney(request.TaxAmount, currency),
            ["AdditionalFees"] = FormatMoney(request.AdditionalFees, currency),
            ["Currency"] = currency,
            ["PaymentMethod"] = request.PaymentMethod,
            ["GradeTrim"] = request.GradeTrim ?? string.Empty,
            ["Transmission"] = request.Transmission,
            ["AuctionGrade"] = request.AuctionGrade ?? string.Empty,
            ["HistoryNotes"] = request.HistoryNotes ?? string.Empty,
            ["Notes"] = request.Notes ?? string.Empty,
        };

        MiniWord.SaveAsByTemplate(outputPath, templatePath, values);

        if (openAfter)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = outputPath,
                UseShellExecute = true
            });
        }

        return Task.FromResult(outputPath);
    }

    private static string FormatInvoiceDate(DateTime date)
        => date.ToString("dd-MMM-yy", CultureInfo.InvariantCulture).ToUpperInvariant();

    /// <summary>Amount with ISO currency code from AppSettings / invoice (e.g. "USD 12,500").</summary>
    private static string FormatMoney(decimal amount, string currency)
        => $"{currency} {amount.ToString("N0", CultureInfo.InvariantCulture)}";

    /// <summary>
    /// Builds a minimal valid .docx (OOXML zip) with MiniWord-style {{Tag}} placeholders
    /// mirroring the MDK export invoice structure.
    /// </summary>
    private static void CreatePlaceholderTemplate(string path)
    {
        var documentXml = """
            <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
            <w:document xmlns:w="http://schemas.openxmlformats.org/wordprocessingml/2006/main">
              <w:body>
                <w:p><w:r><w:rPr><w:b/><w:sz w:val="32"/></w:rPr><w:t>{{AccountHolderName}}</w:t></w:r></w:p>
                <w:p><w:r><w:t>VEHICLE EXPORT INVOICE</w:t></w:r></w:p>
                <w:p><w:r><w:t></w:t></w:r></w:p>
                <w:p><w:r><w:t>Invoice No: {{InvoiceNo}}    Date: {{InvoiceDate}}</w:t></w:r></w:p>
                <w:p><w:r><w:t>Port: {{Port}}    Country: {{Country}}</w:t></w:r></w:p>
                <w:p><w:r><w:t></w:t></w:r></w:p>
                <w:p><w:r><w:rPr><w:b/></w:rPr><w:t>CUSTOMER</w:t></w:r></w:p>
                <w:p><w:r><w:t>{{CustomerName}}</w:t></w:r></w:p>
                <w:p><w:r><w:t>{{CustomerAddress}}</w:t></w:r></w:p>
                <w:p><w:r><w:t></w:t></w:r></w:p>
                <w:p><w:r><w:rPr><w:b/></w:rPr><w:t>CONSIGNEE</w:t></w:r></w:p>
                <w:p><w:r><w:t>{{ConsigneeName}}</w:t></w:r></w:p>
                <w:p><w:r><w:t>{{ConsigneeAddress}}</w:t></w:r></w:p>
                <w:p><w:r><w:t></w:t></w:r></w:p>
                <w:p><w:r><w:rPr><w:b/></w:rPr><w:t>VEHICLE SPECIFICATION</w:t></w:r></w:p>
                <w:tbl>
                  <w:tblPr><w:tblW w:w="5000" w:type="pct"/></w:tblPr>
                  <w:tr>
                    <w:tc><w:p><w:r><w:t>Stock No</w:t></w:r></w:p></w:tc>
                    <w:tc><w:p><w:r><w:t>Maker</w:t></w:r></w:p></w:tc>
                    <w:tc><w:p><w:r><w:t>Chassis</w:t></w:r></w:p></w:tc>
                    <w:tc><w:p><w:r><w:t>CC</w:t></w:r></w:p></w:tc>
                    <w:tc><w:p><w:r><w:t>Color</w:t></w:r></w:p></w:tc>
                    <w:tc><w:p><w:r><w:t>Year</w:t></w:r></w:p></w:tc>
                    <w:tc><w:p><w:r><w:t>Odometer</w:t></w:r></w:p></w:tc>
                    <w:tc><w:p><w:r><w:t>Door</w:t></w:r></w:p></w:tc>
                  </w:tr>
                  <w:tr>
                    <w:tc><w:p><w:r><w:t>{{StockNo}}</w:t></w:r></w:p></w:tc>
                    <w:tc><w:p><w:r><w:t>{{Maker}}</w:t></w:r></w:p></w:tc>
                    <w:tc><w:p><w:r><w:t>{{Chassis}}</w:t></w:r></w:p></w:tc>
                    <w:tc><w:p><w:r><w:t>{{CC}}</w:t></w:r></w:p></w:tc>
                    <w:tc><w:p><w:r><w:t>{{Color}}</w:t></w:r></w:p></w:tc>
                    <w:tc><w:p><w:r><w:t>{{Year}}</w:t></w:r></w:p></w:tc>
                    <w:tc><w:p><w:r><w:t>{{Odometer}}</w:t></w:r></w:p></w:tc>
                    <w:tc><w:p><w:r><w:t>{{Door}}</w:t></w:r></w:p></w:tc>
                  </w:tr>
                  <w:tr>
                    <w:tc><w:p><w:r><w:t>Condition</w:t></w:r></w:p></w:tc>
                    <w:tc><w:p><w:r><w:t>Steering</w:t></w:r></w:p></w:tc>
                    <w:tc><w:p><w:r><w:t>Engine No</w:t></w:r></w:p></w:tc>
                    <w:tc><w:p><w:r><w:t>Seat</w:t></w:r></w:p></w:tc>
                    <w:tc><w:p><w:r><w:t>Body</w:t></w:r></w:p></w:tc>
                    <w:tc><w:p><w:r><w:t>Fuel</w:t></w:r></w:p></w:tc>
                    <w:tc><w:p><w:r><w:t>Unladen Wt</w:t></w:r></w:p></w:tc>
                    <w:tc><w:p><w:r><w:t>{{TradeTerm}}</w:t></w:r></w:p></w:tc>
                  </w:tr>
                  <w:tr>
                    <w:tc><w:p><w:r><w:t>{{Condition}}</w:t></w:r></w:p></w:tc>
                    <w:tc><w:p><w:r><w:t>{{Steering}}</w:t></w:r></w:p></w:tc>
                    <w:tc><w:p><w:r><w:t>{{EngineNo}}</w:t></w:r></w:p></w:tc>
                    <w:tc><w:p><w:r><w:t>{{Seat}}</w:t></w:r></w:p></w:tc>
                    <w:tc><w:p><w:r><w:t>{{Body}}</w:t></w:r></w:p></w:tc>
                    <w:tc><w:p><w:r><w:t>{{Fuel}}</w:t></w:r></w:p></w:tc>
                    <w:tc><w:p><w:r><w:t>{{UnladenWeight}}</w:t></w:r></w:p></w:tc>
                    <w:tc><w:p><w:r><w:t>{{CandF}}</w:t></w:r></w:p></w:tc>
                  </w:tr>
                </w:tbl>
                <w:p><w:r><w:t></w:t></w:r></w:p>
                <w:p><w:r><w:rPr><w:b/></w:rPr><w:t>BANK DETAILS</w:t></w:r></w:p>
                <w:p><w:r><w:t>Bank: {{BankName}}</w:t></w:r></w:p>
                <w:p><w:r><w:t>Account Holder: {{AccountHolderName}}</w:t></w:r></w:p>
                <w:p><w:r><w:t>Account No: {{AccountNo}}</w:t></w:r></w:p>
                <w:p><w:r><w:t>Branch: {{Branch}} ({{BranchCode}})</w:t></w:r></w:p>
                <w:p><w:r><w:t>Branch Address: {{BranchAddress}}</w:t></w:r></w:p>
                <w:p><w:r><w:t>SWIFT: {{SwiftCode}}</w:t></w:r></w:p>
                <w:p><w:r><w:t></w:t></w:r></w:p>
                <w:p><w:r><w:rPr><w:b/></w:rPr><w:t>INTERMEDIARY BANK</w:t></w:r></w:p>
                <w:p><w:r><w:t>{{IntermediaryBankName}} — {{IntermediaryCountry}}</w:t></w:r></w:p>
                <w:p><w:r><w:t>{{IntermediaryBranch}}  SWIFT: {{IntermediarySwiftCode}}</w:t></w:r></w:p>
                <w:p><w:r><w:t></w:t></w:r></w:p>
                <w:p><w:r><w:rPr><w:b/><w:sz w:val="28"/></w:rPr><w:t>TOTAL {{Currency}}: {{TotalAmount}}</w:t></w:r></w:p>
                <w:p><w:r><w:t></w:t></w:r></w:p>
                <w:p><w:r><w:t>We hereby certify that the above particulars are true and correct.</w:t></w:r></w:p>
                <w:p><w:r><w:t>This vehicle is exported used for commercial purposes.</w:t></w:r></w:p>
                <w:sectPr/>
              </w:body>
            </w:document>
            """;

        var contentTypes = """
            <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
            <Types xmlns="http://schemas.openxmlformats.org/package/2006/content-types">
              <Default Extension="rels" ContentType="application/vnd.openxmlformats-package.relationships+xml"/>
              <Default Extension="xml" ContentType="application/xml"/>
              <Override PartName="/word/document.xml" ContentType="application/vnd.openxmlformats-officedocument.wordprocessingml.document.main+xml"/>
            </Types>
            """;

        var rels = """
            <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
            <Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
              <Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument" Target="word/document.xml"/>
            </Relationships>
            """;

        if (File.Exists(path)) File.Delete(path);
        using var zip = ZipFile.Open(path, ZipArchiveMode.Create);
        WriteEntry(zip, "[Content_Types].xml", contentTypes);
        WriteEntry(zip, "_rels/.rels", rels);
        WriteEntry(zip, "word/document.xml", documentXml);
    }

    private static void WriteEntry(ZipArchive zip, string entryName, string content)
    {
        var entry = zip.CreateEntry(entryName, CompressionLevel.Optimal);
        using var stream = entry.Open();
        using var writer = new StreamWriter(stream, new UTF8Encoding(false));
        writer.Write(content);
    }
}
