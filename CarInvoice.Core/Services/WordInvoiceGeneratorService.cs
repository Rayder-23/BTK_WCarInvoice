using System.Diagnostics;
using System.IO.Compression;
using System.Text;
using CarInvoice.Core.DTOs;
using CarInvoice.Core.Interfaces;
using MiniSoftware;

namespace CarInvoice.Core.Services;

/// <summary>
/// Generates invoice .docx files from a MiniWord template, creating a clean
/// placeholder template on first run when none exists.
/// </summary>
public sealed class WordInvoiceGeneratorService : IWordInvoiceGeneratorService
{
    /// <inheritdoc />
    public string EnsureDefaultTemplate(string templatesDirectory)
    {
        Directory.CreateDirectory(templatesDirectory);
        var path = Path.Combine(templatesDirectory, "DefaultInvoiceTemplate.docx");
        if (!File.Exists(path))
            CreatePlaceholderTemplate(path);
        return path;
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
        var safeNumber = string.Join("_", request.InvoiceNumber.Split(Path.GetInvalidFileNameChars()));
        var outputPath = Path.Combine(outputDirectory, $"{safeNumber}_{DateTime.Now:yyyyMMdd_HHmmss}.docx");

        var values = new Dictionary<string, object>
        {
            ["InvoiceNumber"] = request.InvoiceNumber,
            ["InvoiceDate"] = request.InvoiceDate.ToString("dd MMM yyyy"),
            ["CustomerName"] = request.CustomerName,
            ["CustomerPhone"] = request.CustomerPhone,
            ["CustomerEmail"] = request.CustomerEmail,
            ["CustomerAddress"] = request.CustomerAddress,
            ["CompanyName"] = request.CompanyName ?? string.Empty,
            ["Make"] = request.Make,
            ["Model"] = request.Model,
            ["GradeTrim"] = request.GradeTrim ?? string.Empty,
            ["Year"] = request.Year.ToString(),
            ["ChassisCode"] = request.ChassisCode,
            ["ChassisNumber"] = request.ChassisNumber,
            ["EngineCc"] = request.EngineCc?.ToString() ?? string.Empty,
            ["EngineNumber"] = request.EngineNumber ?? string.Empty,
            ["Color"] = request.Color ?? string.Empty,
            ["Mileage"] = $"{request.Mileage:N0} {request.MileageUnit}",
            ["FuelType"] = request.FuelType,
            ["Transmission"] = request.Transmission,
            ["AuctionGrade"] = request.AuctionGrade ?? string.Empty,
            ["HistoryNotes"] = request.HistoryNotes ?? string.Empty,
            ["SalePrice"] = FormatMoney(request.SalePrice, request.Currency),
            ["TaxAmount"] = FormatMoney(request.TaxAmount, request.Currency),
            ["AdditionalFees"] = FormatMoney(request.AdditionalFees, request.Currency),
            ["TotalAmount"] = FormatMoney(request.TotalAmount, request.Currency),
            ["Currency"] = request.Currency,
            ["PaymentMethod"] = request.PaymentMethod,
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

    private static string FormatMoney(decimal amount, string currency)
        => $"{currency} {amount:N2}";

    /// <summary>
    /// Builds a minimal valid .docx (OOXML zip) with MiniWord-style {{Tag}} placeholders.
    /// </summary>
    private static void CreatePlaceholderTemplate(string path)
    {
        var documentXml = """
            <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
            <w:document xmlns:w="http://schemas.openxmlformats.org/wordprocessingml/2006/main">
              <w:body>
                <w:p><w:r><w:rPr><w:b/><w:sz w:val="36"/></w:rPr><w:t>VEHICLE SALE INVOICE</w:t></w:r></w:p>
                <w:p><w:r><w:t>Invoice No: {{InvoiceNumber}}</w:t></w:r></w:p>
                <w:p><w:r><w:t>Date: {{InvoiceDate}}</w:t></w:r></w:p>
                <w:p><w:r><w:t></w:t></w:r></w:p>
                <w:p><w:r><w:rPr><w:b/></w:rPr><w:t>CUSTOMER</w:t></w:r></w:p>
                <w:p><w:r><w:t>Name: {{CustomerName}}</w:t></w:r></w:p>
                <w:p><w:r><w:t>Company: {{CompanyName}}</w:t></w:r></w:p>
                <w:p><w:r><w:t>Phone: {{CustomerPhone}}</w:t></w:r></w:p>
                <w:p><w:r><w:t>Email: {{CustomerEmail}}</w:t></w:r></w:p>
                <w:p><w:r><w:t>Address: {{CustomerAddress}}</w:t></w:r></w:p>
                <w:p><w:r><w:t></w:t></w:r></w:p>
                <w:p><w:r><w:rPr><w:b/></w:rPr><w:t>VEHICLE</w:t></w:r></w:p>
                <w:p><w:r><w:t>Make / Model: {{Make}} {{Model}} {{GradeTrim}}</w:t></w:r></w:p>
                <w:p><w:r><w:t>Year: {{Year}}</w:t></w:r></w:p>
                <w:p><w:r><w:t>Chassis: {{ChassisCode}}-{{ChassisNumber}}</w:t></w:r></w:p>
                <w:p><w:r><w:t>Engine: {{EngineCc}} cc  No: {{EngineNumber}}</w:t></w:r></w:p>
                <w:p><w:r><w:t>Color: {{Color}}</w:t></w:r></w:p>
                <w:p><w:r><w:t>Mileage: {{Mileage}}</w:t></w:r></w:p>
                <w:p><w:r><w:t>Fuel / Transmission: {{FuelType}} / {{Transmission}}</w:t></w:r></w:p>
                <w:p><w:r><w:t>Auction Grade: {{AuctionGrade}}</w:t></w:r></w:p>
                <w:p><w:r><w:t>History: {{HistoryNotes}}</w:t></w:r></w:p>
                <w:p><w:r><w:t></w:t></w:r></w:p>
                <w:p><w:r><w:rPr><w:b/></w:rPr><w:t>PRICING</w:t></w:r></w:p>
                <w:p><w:r><w:t>Sale Price: {{SalePrice}}</w:t></w:r></w:p>
                <w:p><w:r><w:t>Tax: {{TaxAmount}}</w:t></w:r></w:p>
                <w:p><w:r><w:t>Additional Fees: {{AdditionalFees}}</w:t></w:r></w:p>
                <w:p><w:r><w:t>Total: {{TotalAmount}}</w:t></w:r></w:p>
                <w:p><w:r><w:t>Payment: {{PaymentMethod}}</w:t></w:r></w:p>
                <w:p><w:r><w:t>Notes: {{Notes}}</w:t></w:r></w:p>
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
