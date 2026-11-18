using CarInvoice.Core.DTOs;
using CarInvoice.Core.Entities;
using CarInvoice.Core.Interfaces;
using ClosedXML.Excel;

namespace CarInvoice.Core.Services;

/// <summary>
/// ClosedXML-based Excel import/export with flexible English and Japanese column header mapping.
/// </summary>
public sealed class ExcelService : IExcelService
{
    private readonly IJapaneseTranslationService _translator;
    private readonly IChassisParserService _chassisParser;

    private static readonly Dictionary<string, string[]> HeaderAliases = new(StringComparer.OrdinalIgnoreCase)
    {
        ["ChassisCode"] = new[] { "ChassisCode", "Chassis Code", "型式", "Model Code", "Code" },
        ["ChassisNumber"] = new[] { "ChassisNumber", "Chassis Number", "Chassis No", "車台番号", "Frame No", "VIN" },
        ["Make"] = new[] { "Make", "Maker", "メーカー", "Brand" },
        ["Model"] = new[] { "Model", "車名", "車種", "Model Name" },
        ["Year"] = new[] { "Year", "年式", "Model Year", "Manufacturing Year" },
        ["Mileage"] = new[] { "Mileage", "走行距離", "Odometer", "KM", "Km" },
        ["Color"] = new[] { "Color", "Colour", "色", "Exterior Color", "外装色" },
        ["Price"] = new[] { "Price", "Sale Price", "価格", "売価", "Amount" },
        ["CustomerName"] = new[] { "Customer", "Customer Name", "Buyer", "お客様", "買主" },
        ["FuelType"] = new[] { "Fuel", "Fuel Type", "燃料" },
        ["Transmission"] = new[] { "Transmission", "シフト", "ミッション" },
        ["AuctionGrade"] = new[] { "Auction Grade", "Grade", "評価点" },
        ["HistoryNotes"] = new[] { "History", "Notes", "備考", "History Notes" },
        ["EngineCc"] = new[] { "Engine Cc", "CC", "排気量", "Engine" },
        ["GradeTrim"] = new[] { "Grade Trim", "Trim", "グレード" },
    };

    public ExcelService(IJapaneseTranslationService translator, IChassisParserService chassisParser)
    {
        _translator = translator;
        _chassisParser = chassisParser;
    }

    /// <inheritdoc />
    public async Task<List<ExtractedVehicleFields>> ImportVehiclesAsync(string filePath, CancellationToken ct = default)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException("Excel file not found.", filePath);

        var results = new List<ExtractedVehicleFields>();
        using var workbook = new XLWorkbook(filePath);
        var sheet = workbook.Worksheets.First();
        var headerRow = sheet.FirstRowUsed() ?? throw new InvalidOperationException("Worksheet is empty.");
        var map = BuildColumnMap(headerRow);

        foreach (var row in sheet.RowsUsed().Skip(1))
        {
            ct.ThrowIfCancellationRequested();
            var fields = new ExtractedVehicleFields
            {
                ChassisCode = GetCell(row, map, "ChassisCode"),
                ChassisNumber = GetCell(row, map, "ChassisNumber"),
                Make = GetCell(row, map, "Make"),
                Model = GetCell(row, map, "Model"),
                Color = GetCell(row, map, "Color"),
                FuelType = GetCell(row, map, "FuelType"),
                Transmission = GetCell(row, map, "Transmission"),
                AuctionGrade = GetCell(row, map, "AuctionGrade"),
                CustomerName = GetCell(row, map, "CustomerName"),
                GradeTrim = GetCell(row, map, "GradeTrim"),
                JapaneseRemarks = GetCell(row, map, "HistoryNotes"),
            };

            if (int.TryParse(GetCell(row, map, "Year")?.Replace(",", ""), out var year))
                fields.Year = year;
            if (int.TryParse(GetCell(row, map, "Mileage")?.Replace(",", ""), out var mileage))
                fields.Mileage = mileage;
            if (decimal.TryParse(GetCell(row, map, "Price")?.Replace(",", ""), out var price))
                fields.Price = price;

            var chassisRaw = string.Join(" ", new[] { fields.ChassisCode, fields.ChassisNumber }.Where(s => !string.IsNullOrWhiteSpace(s)));
            if (!string.IsNullOrWhiteSpace(chassisRaw))
            {
                var parsed = await _chassisParser.ParseAsync(chassisRaw, ct);
                fields.ChassisCode = string.IsNullOrWhiteSpace(parsed.ChassisCode) ? fields.ChassisCode : parsed.ChassisCode;
                fields.ChassisNumber = string.IsNullOrWhiteSpace(parsed.ChassisNumber) ? fields.ChassisNumber : parsed.ChassisNumber;
                fields.Make ??= parsed.SuggestedMake;
                fields.Model ??= parsed.SuggestedModel;
                fields.FuelType ??= parsed.SuggestedFuelType;
            }

            if (!string.IsNullOrWhiteSpace(fields.Color))
                fields.Color = await _translator.TranslateAsync(fields.Color, false, ct);
            if (!string.IsNullOrWhiteSpace(fields.JapaneseRemarks))
                fields.TranslatedRemarks = await _translator.TranslateAsync(fields.JapaneseRemarks, true, ct);

            // Skip completely empty rows
            if (string.IsNullOrWhiteSpace(fields.ChassisNumber) && string.IsNullOrWhiteSpace(fields.Model) && fields.Price is null)
                continue;

            results.Add(fields);
        }

        return results;
    }

    /// <inheritdoc />
    public Task ExportInvoicesAsync(IEnumerable<Invoice> invoices, string filePath, CancellationToken ct = default)
    {
        using var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add("Invoices");
        var headers = new[]
        {
            "Invoice Number", "Invoice Date", "Customer", "Phone", "Make", "Model",
            "Chassis Code", "Chassis Number", "Year", "Mileage", "Sale Price", "Tax",
            "Fees", "Total", "Currency", "Payment", "Notes", "Doc Path"
        };
        for (var i = 0; i < headers.Length; i++)
            sheet.Cell(1, i + 1).Value = headers[i];

        var row = 2;
        foreach (var inv in invoices)
        {
            ct.ThrowIfCancellationRequested();
            sheet.Cell(row, 1).Value = inv.InvoiceNumber;
            sheet.Cell(row, 2).Value = inv.InvoiceDate;
            sheet.Cell(row, 3).Value = inv.Customer?.FullName ?? string.Empty;
            sheet.Cell(row, 4).Value = inv.Customer?.Phone ?? string.Empty;
            sheet.Cell(row, 5).Value = inv.Vehicle?.Make ?? string.Empty;
            sheet.Cell(row, 6).Value = inv.Vehicle?.Model ?? string.Empty;
            sheet.Cell(row, 7).Value = inv.Vehicle?.ChassisCode ?? string.Empty;
            sheet.Cell(row, 8).Value = inv.Vehicle?.ChassisNumber ?? string.Empty;
            sheet.Cell(row, 9).Value = inv.Vehicle?.ManufacturingYear ?? 0;
            sheet.Cell(row, 10).Value = inv.Vehicle?.Mileage ?? 0;
            sheet.Cell(row, 11).Value = inv.SalePrice;
            sheet.Cell(row, 12).Value = inv.TaxAmount;
            sheet.Cell(row, 13).Value = inv.AdditionalFees;
            sheet.Cell(row, 14).Value = inv.TotalAmount;
            sheet.Cell(row, 15).Value = inv.Currency;
            sheet.Cell(row, 16).Value = inv.PaymentMethod;
            sheet.Cell(row, 17).Value = inv.Notes ?? string.Empty;
            sheet.Cell(row, 18).Value = inv.GeneratedDocPath ?? string.Empty;
            row++;
        }

        StyleHeader(sheet, headers.Length);
        sheet.Columns().AdjustToContents();
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(filePath))!);
        workbook.SaveAs(filePath);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task ExportVehiclesAsync(IEnumerable<Vehicle> vehicles, string filePath, CancellationToken ct = default)
    {
        using var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add("Vehicles");
        var headers = new[]
        {
            "Chassis Code", "Chassis Number", "Make", "Model", "Grade", "Year", "Engine Cc",
            "Color", "Fuel", "Transmission", "Mileage", "Unit", "Auction Grade", "History", "Updated"
        };
        for (var i = 0; i < headers.Length; i++)
            sheet.Cell(1, i + 1).Value = headers[i];

        var row = 2;
        foreach (var v in vehicles)
        {
            ct.ThrowIfCancellationRequested();
            sheet.Cell(row, 1).Value = v.ChassisCode;
            sheet.Cell(row, 2).Value = v.ChassisNumber;
            sheet.Cell(row, 3).Value = v.Make;
            sheet.Cell(row, 4).Value = v.Model;
            sheet.Cell(row, 5).Value = v.GradeTrim ?? string.Empty;
            sheet.Cell(row, 6).Value = v.ManufacturingYear;
            sheet.Cell(row, 7).Value = v.EngineCapacityCc ?? 0;
            sheet.Cell(row, 8).Value = v.ExteriorColor ?? string.Empty;
            sheet.Cell(row, 9).Value = v.FuelType;
            sheet.Cell(row, 10).Value = v.Transmission;
            sheet.Cell(row, 11).Value = v.Mileage;
            sheet.Cell(row, 12).Value = v.MileageUnit;
            sheet.Cell(row, 13).Value = v.AuctionGrade ?? string.Empty;
            sheet.Cell(row, 14).Value = v.HistoryNotes ?? string.Empty;
            sheet.Cell(row, 15).Value = v.UpdatedAt;
            row++;
        }

        StyleHeader(sheet, headers.Length);
        sheet.Columns().AdjustToContents();
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(filePath))!);
        workbook.SaveAs(filePath);
        return Task.CompletedTask;
    }

    private static Dictionary<string, int> BuildColumnMap(IXLRow headerRow)
    {
        var map = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        foreach (var cell in headerRow.CellsUsed())
        {
            var header = cell.GetString().Trim();
            foreach (var (field, aliases) in HeaderAliases)
            {
                if (aliases.Any(a => string.Equals(a, header, StringComparison.OrdinalIgnoreCase)))
                {
                    map[field] = cell.Address.ColumnNumber;
                    break;
                }
            }
        }
        return map;
    }

    private static string? GetCell(IXLRow row, Dictionary<string, int> map, string field)
    {
        if (!map.TryGetValue(field, out var col)) return null;
        var value = row.Cell(col).GetFormattedString()?.Trim();
        return string.IsNullOrWhiteSpace(value) ? null : value;
    }

    private static void StyleHeader(IXLWorksheet sheet, int columnCount)
    {
        var range = sheet.Range(1, 1, 1, columnCount);
        range.Style.Font.Bold = true;
        range.Style.Fill.BackgroundColor = XLColor.FromHtml("#1F4E79");
        range.Style.Font.FontColor = XLColor.White;
    }
}
