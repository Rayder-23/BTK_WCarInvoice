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
        ["StockNumber"] = new[] { "Stock", "Stock No", "Stock Number", "Stock #", "ストック" },
        ["ChassisCode"] = new[] { "ChassisCode", "Chassis Code", "型式", "Model Code", "Code" },
        ["ChassisNumber"] = new[] { "ChassisNumber", "Chassis Number", "Chassis No", "Chassis", "車台番号", "Frame No", "VIN" },
        ["Make"] = new[] { "Make", "Maker", "メーカー", "Brand" },
        ["Model"] = new[] { "Model", "車名", "車種", "Model Name" },
        ["Year"] = new[] { "Year", "年式", "Model Year", "Manufacturing Year" },
        ["Mileage"] = new[] { "Mileage", "走行距離", "Odometer", "Odo", "KM", "Km" },
        ["Color"] = new[] { "Color", "Colour", "色", "Exterior Color", "外装色" },
        ["Price"] = new[] { "Price", "Sale Price", "C & F", "C&F", "FOB", "CIF", "価格", "売価", "Amount" },
        ["CustomerName"] = new[] { "Customer", "Customer Name", "Buyer", "お客様", "買主" },
        ["CustomerAddress"] = new[] { "Customer Address", "Buyer Address", "Address" },
        ["ConsigneeName"] = new[] { "Consignee", "Consignee Name" },
        ["ConsigneeAddress"] = new[] { "Consignee Address" },
        ["DestinationPort"] = new[] { "Port", "Destination Port", "Port of Discharge" },
        ["DestinationCountry"] = new[] { "Country", "Destination Country" },
        ["TradeTerm"] = new[] { "Trade Term", "Incoterm", "Term" },
        ["FuelType"] = new[] { "Fuel", "Fuel Type", "燃料" },
        ["Transmission"] = new[] { "Transmission", "シフト", "ミッション" },
        ["AuctionGrade"] = new[] { "Auction Grade", "Grade", "評価点" },
        ["HistoryNotes"] = new[] { "History", "Notes", "備考", "History Notes" },
        ["EngineCc"] = new[] { "Engine Cc", "CC", "排気量", "Engine" },
        ["EngineNumber"] = new[] { "Engine No", "Engine Number", "エンジン番号" },
        ["Doors"] = new[] { "Door", "Doors", "ドア" },
        ["Seats"] = new[] { "Seat", "Seats", "座席" },
        ["Condition"] = new[] { "Condition", "状態" },
        ["Steering"] = new[] { "Steering", "ハンドル" },
        ["BodyType"] = new[] { "Body", "Body Type", "ボディ" },
        ["UnladenWeightKg"] = new[] { "Unladen Weight", "Curb Weight", "車両重量", "Weight" },
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
                StockNumber = GetCell(row, map, "StockNumber"),
                ChassisCode = GetCell(row, map, "ChassisCode"),
                ChassisNumber = GetCell(row, map, "ChassisNumber"),
                Make = GetCell(row, map, "Make"),
                Model = GetCell(row, map, "Model"),
                Color = GetCell(row, map, "Color"),
                FuelType = GetCell(row, map, "FuelType"),
                Transmission = GetCell(row, map, "Transmission"),
                AuctionGrade = GetCell(row, map, "AuctionGrade"),
                CustomerName = GetCell(row, map, "CustomerName"),
                CustomerAddress = GetCell(row, map, "CustomerAddress"),
                ConsigneeName = GetCell(row, map, "ConsigneeName"),
                ConsigneeAddress = GetCell(row, map, "ConsigneeAddress"),
                DestinationPort = GetCell(row, map, "DestinationPort"),
                DestinationCountry = GetCell(row, map, "DestinationCountry"),
                TradeTerm = GetCell(row, map, "TradeTerm"),
                EngineNumber = GetCell(row, map, "EngineNumber"),
                Condition = GetCell(row, map, "Condition"),
                Steering = GetCell(row, map, "Steering"),
                BodyType = GetCell(row, map, "BodyType"),
                GradeTrim = GetCell(row, map, "GradeTrim"),
                JapaneseRemarks = GetCell(row, map, "HistoryNotes"),
            };

            if (int.TryParse(GetCell(row, map, "Year")?.Replace(",", ""), out var year))
                fields.Year = year;
            if (int.TryParse(GetCell(row, map, "Mileage")?.Replace(",", ""), out var mileage))
                fields.Mileage = mileage;
            if (decimal.TryParse(GetCell(row, map, "Price")?.Replace(",", ""), out var price))
                fields.Price = price;
            if (int.TryParse(GetCell(row, map, "EngineCc")?.Replace(",", ""), out var engineCc))
                fields.EngineCc = engineCc;
            if (int.TryParse(GetCell(row, map, "Doors")?.Replace(",", ""), out var doors))
                fields.Doors = doors;
            if (int.TryParse(GetCell(row, map, "Seats")?.Replace(",", ""), out var seats))
                fields.Seats = seats;
            if (int.TryParse(GetCell(row, map, "UnladenWeightKg")?.Replace(",", ""), out var uw))
                fields.UnladenWeightKg = uw;

            if (string.IsNullOrWhiteSpace(fields.ConsigneeName))
                fields.ConsigneeName = fields.CustomerName;
            if (string.IsNullOrWhiteSpace(fields.ConsigneeAddress))
                fields.ConsigneeAddress = fields.CustomerAddress;

            var chassisRaw = string.Join(" ", new[] { fields.ChassisCode, fields.ChassisNumber }.Where(s => !string.IsNullOrWhiteSpace(s)));
            if (!string.IsNullOrWhiteSpace(chassisRaw))
            {
                var parsed = await _chassisParser.ParseAsync(chassisRaw, ct);
                fields.ChassisCode = string.IsNullOrWhiteSpace(parsed.ChassisCode) ? fields.ChassisCode : parsed.ChassisCode;
                fields.ChassisNumber = string.IsNullOrWhiteSpace(parsed.ChassisNumber) ? fields.ChassisNumber : parsed.ChassisNumber;
                fields.Make ??= parsed.SuggestedMake;
                fields.Model ??= parsed.SuggestedModel;
                fields.FuelType ??= parsed.SuggestedFuelType;
                fields.EngineCc ??= parsed.SuggestedEngineCc;
                fields.BodyType ??= parsed.SuggestedBodyType;
                fields.Doors ??= parsed.SuggestedDoors;
                fields.Seats ??= parsed.SuggestedSeats;
                fields.EngineNumber ??= parsed.SuggestedEngineNumber;
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
            "Invoice Number", "Invoice Date", "Port", "Country", "Customer", "Consignee",
            "Make", "Model", "Chassis Code", "Chassis Number", "Stock No", "Year", "Mileage",
            "Trade Term", "Price", "Total", "Currency", "Notes", "Doc Path"
        };
        for (var i = 0; i < headers.Length; i++)
            sheet.Cell(1, i + 1).Value = headers[i];

        var row = 2;
        foreach (var inv in invoices)
        {
            ct.ThrowIfCancellationRequested();
            sheet.Cell(row, 1).Value = inv.InvoiceNumber;
            sheet.Cell(row, 2).Value = inv.InvoiceDate;
            sheet.Cell(row, 3).Value = inv.DestinationPort ?? string.Empty;
            sheet.Cell(row, 4).Value = inv.DestinationCountry ?? string.Empty;
            sheet.Cell(row, 5).Value = inv.Customer?.FullName ?? string.Empty;
            sheet.Cell(row, 6).Value = inv.ConsigneeName ?? inv.Customer?.FullName ?? string.Empty;
            sheet.Cell(row, 7).Value = inv.Vehicle?.Make ?? string.Empty;
            sheet.Cell(row, 8).Value = inv.Vehicle?.Model ?? string.Empty;
            sheet.Cell(row, 9).Value = inv.Vehicle?.ChassisCode ?? string.Empty;
            sheet.Cell(row, 10).Value = inv.Vehicle?.ChassisNumber ?? string.Empty;
            sheet.Cell(row, 11).Value = inv.Vehicle?.StockNumber ?? string.Empty;
            sheet.Cell(row, 12).Value = inv.Vehicle?.ManufacturingYear ?? 0;
            sheet.Cell(row, 13).Value = inv.Vehicle?.Mileage ?? 0;
            sheet.Cell(row, 14).Value = inv.TradeTerm;
            sheet.Cell(row, 15).Value = inv.PriceAmount > 0 ? inv.PriceAmount : inv.SalePrice;
            sheet.Cell(row, 16).Value = inv.TotalAmount;
            sheet.Cell(row, 17).Value = inv.Currency;
            sheet.Cell(row, 18).Value = inv.Notes ?? string.Empty;
            sheet.Cell(row, 19).Value = inv.GeneratedDocPath ?? string.Empty;
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
            "Stock No", "Chassis Code", "Chassis Number", "Make", "Model", "Year", "Engine Cc",
            "Color", "Fuel", "Mileage", "Unit", "Doors", "Seats", "Condition", "Steering",
            "Body", "Engine No", "Unladen Weight", "History", "Updated"
        };
        for (var i = 0; i < headers.Length; i++)
            sheet.Cell(1, i + 1).Value = headers[i];

        var row = 2;
        foreach (var v in vehicles)
        {
            ct.ThrowIfCancellationRequested();
            sheet.Cell(row, 1).Value = v.StockNumber ?? string.Empty;
            sheet.Cell(row, 2).Value = v.ChassisCode;
            sheet.Cell(row, 3).Value = v.ChassisNumber;
            sheet.Cell(row, 4).Value = v.Make;
            sheet.Cell(row, 5).Value = v.Model;
            sheet.Cell(row, 6).Value = v.ManufacturingYear;
            sheet.Cell(row, 7).Value = v.EngineCapacityCc ?? 0;
            sheet.Cell(row, 8).Value = v.ExteriorColor ?? string.Empty;
            sheet.Cell(row, 9).Value = v.FuelType;
            sheet.Cell(row, 10).Value = v.Mileage;
            sheet.Cell(row, 11).Value = v.MileageUnit;
            sheet.Cell(row, 12).Value = v.Doors ?? 0;
            sheet.Cell(row, 13).Value = v.Seats ?? 0;
            sheet.Cell(row, 14).Value = v.Condition;
            sheet.Cell(row, 15).Value = v.Steering;
            sheet.Cell(row, 16).Value = v.BodyType ?? string.Empty;
            sheet.Cell(row, 17).Value = v.EngineNumber ?? string.Empty;
            sheet.Cell(row, 18).Value = v.UnladenWeightKg ?? 0;
            sheet.Cell(row, 19).Value = v.HistoryNotes ?? string.Empty;
            sheet.Cell(row, 20).Value = v.UpdatedAt;
            row++;
        }

        StyleHeader(sheet, headers.Length);
        sheet.Columns().AdjustToContents();
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(filePath))!);
        workbook.SaveAs(filePath);
        return Task.CompletedTask;
    }

    private static readonly string[] ChassisLookupHeaders =
    {
        "ChassisCode", "DefaultMake", "DefaultModel", "SerialPrefixOrRange", "DefaultEngineCc",
        "DefaultFuelType", "DefaultBodyType", "DefaultDoors", "DefaultSeats", "DefaultEngineNo", "Notes"
    };

    /// <inheritdoc />
    public Task ExportChassisLookupsAsync(IEnumerable<ChassisCodeLookup> rows, string filePath, CancellationToken ct = default)
    {
        using var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add("ChassisLookups");
        for (var i = 0; i < ChassisLookupHeaders.Length; i++)
            sheet.Cell(1, i + 1).Value = ChassisLookupHeaders[i];

        var row = 2;
        foreach (var c in rows)
        {
            ct.ThrowIfCancellationRequested();
            sheet.Cell(row, 1).Value = c.ChassisCode;
            sheet.Cell(row, 2).Value = c.DefaultMake;
            sheet.Cell(row, 3).Value = c.DefaultModel;
            sheet.Cell(row, 4).Value = c.SerialPrefixOrRange ?? string.Empty;
            sheet.Cell(row, 5).Value = c.DefaultEngineCc ?? 0;
            sheet.Cell(row, 6).Value = c.DefaultFuelType;
            sheet.Cell(row, 7).Value = c.DefaultBodyType ?? string.Empty;
            sheet.Cell(row, 8).Value = c.DefaultDoors ?? 0;
            sheet.Cell(row, 9).Value = c.DefaultSeats ?? 0;
            sheet.Cell(row, 10).Value = c.DefaultEngineNumber ?? string.Empty;
            sheet.Cell(row, 11).Value = c.Notes ?? string.Empty;
            row++;
        }

        StyleHeader(sheet, ChassisLookupHeaders.Length);
        sheet.Columns().AdjustToContents();
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(filePath))!);
        workbook.SaveAs(filePath);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public async Task<DictionaryImportResult> ImportChassisLookupsAsync(
        string filePath,
        IChassisLookupRepository repo,
        CancellationToken ct = default)
    {
        var result = new DictionaryImportResult();
        using var workbook = new XLWorkbook(filePath);
        var sheet = workbook.Worksheets.First();
        var headerRow = sheet.FirstRowUsed() ?? throw new InvalidOperationException("Worksheet is empty.");
        var map = BuildExactHeaderMap(headerRow, ChassisLookupHeaders);
        if (!map.ContainsKey("ChassisCode") || !map.ContainsKey("DefaultMake") || !map.ContainsKey("DefaultModel"))
            throw new InvalidOperationException("Required columns: ChassisCode, DefaultMake, DefaultModel.");

        foreach (var row in sheet.RowsUsed().Skip(1))
        {
            ct.ThrowIfCancellationRequested();
            var code = GetCell(row, map, "ChassisCode")?.ToUpperInvariant();
            var make = GetCell(row, map, "DefaultMake");
            var model = GetCell(row, map, "DefaultModel");
            if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(make) || string.IsNullOrWhiteSpace(model))
            {
                result.Skipped++;
                continue;
            }

            var serial = GetCell(row, map, "SerialPrefixOrRange");
            var existing = await repo.FindExactAsync(code, make, model, serial, ct);
            if (existing is null)
            {
                existing = new ChassisCodeLookup { ChassisCode = code, DefaultMake = make, DefaultModel = model };
                await repo.AddAsync(existing, ct);
                result.Inserted++;
            }
            else
            {
                repo.Update(existing);
                result.Updated++;
            }

            existing.SerialPrefixOrRange = serial;
            existing.Notes = GetCell(row, map, "Notes");
            if (int.TryParse(GetCell(row, map, "DefaultEngineCc")?.Replace(",", ""), out var cc))
                existing.DefaultEngineCc = cc;
            existing.DefaultFuelType = GetCell(row, map, "DefaultFuelType") ?? existing.DefaultFuelType;
            existing.DefaultBodyType = GetCell(row, map, "DefaultBodyType");
            if (int.TryParse(GetCell(row, map, "DefaultDoors"), out var doors))
                existing.DefaultDoors = doors;
            if (int.TryParse(GetCell(row, map, "DefaultSeats"), out var seats))
                existing.DefaultSeats = seats;
            existing.DefaultEngineNumber = GetCell(row, map, "DefaultEngineNo");
        }

        return result;
    }

    /// <inheritdoc />
    public Task ExportGlossaryAsync(IEnumerable<JapaneseTermGlossary> rows, string filePath, CancellationToken ct = default)
    {
        using var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add("Glossary");
        var headers = new[] { "JapaneseTerm", "EnglishTranslation", "Category" };
        for (var i = 0; i < headers.Length; i++)
            sheet.Cell(1, i + 1).Value = headers[i];

        var row = 2;
        foreach (var g in rows)
        {
            ct.ThrowIfCancellationRequested();
            sheet.Cell(row, 1).Value = g.JapaneseTerm;
            sheet.Cell(row, 2).Value = g.EnglishTranslation;
            sheet.Cell(row, 3).Value = g.Category;
            row++;
        }

        StyleHeader(sheet, headers.Length);
        sheet.Columns().AdjustToContents();
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(filePath))!);
        workbook.SaveAs(filePath);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public async Task<DictionaryImportResult> ImportGlossaryAsync(
        string filePath,
        IGlossaryRepository repo,
        CancellationToken ct = default)
    {
        var result = new DictionaryImportResult();
        using var workbook = new XLWorkbook(filePath);
        var sheet = workbook.Worksheets.First();
        var headerRow = sheet.FirstRowUsed() ?? throw new InvalidOperationException("Worksheet is empty.");
        var map = BuildExactHeaderMap(headerRow, new[] { "JapaneseTerm", "EnglishTranslation", "Category" });
        if (!map.ContainsKey("JapaneseTerm") || !map.ContainsKey("EnglishTranslation"))
            throw new InvalidOperationException("Required columns: JapaneseTerm, EnglishTranslation.");

        foreach (var row in sheet.RowsUsed().Skip(1))
        {
            ct.ThrowIfCancellationRequested();
            var jp = GetCell(row, map, "JapaneseTerm");
            var en = GetCell(row, map, "EnglishTranslation");
            if (string.IsNullOrWhiteSpace(jp) || string.IsNullOrWhiteSpace(en))
            {
                result.Skipped++;
                continue;
            }

            var existing = await repo.FindByJapaneseTermAsync(jp, ct);
            if (existing is null)
            {
                existing = new JapaneseTermGlossary { JapaneseTerm = jp };
                await repo.AddAsync(existing, ct);
                result.Inserted++;
            }
            else
            {
                repo.Update(existing);
                result.Updated++;
            }

            existing.EnglishTranslation = en;
            existing.Category = GetCell(row, map, "Category") ?? "Grade/Spec";
        }

        return result;
    }

    /// <inheritdoc />
    public Task ExportBankProfilesAsync(IEnumerable<BankProfile> rows, string filePath, CancellationToken ct = default)
    {
        using var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add("BankProfiles");
        var headers = new[]
        {
            "ProfileName", "IsDefault", "BankName", "AccountHolderName", "AccountNumber",
            "BranchName", "BranchCode", "BranchAddress", "SwiftCode",
            "IntermediaryBankName", "IntermediaryCountry", "IntermediaryBranch", "IntermediarySwiftCode"
        };
        for (var i = 0; i < headers.Length; i++)
            sheet.Cell(1, i + 1).Value = headers[i];

        var row = 2;
        foreach (var b in rows)
        {
            ct.ThrowIfCancellationRequested();
            sheet.Cell(row, 1).Value = b.ProfileName;
            sheet.Cell(row, 2).Value = b.IsDefault;
            sheet.Cell(row, 3).Value = b.BankName;
            sheet.Cell(row, 4).Value = b.AccountHolderName;
            sheet.Cell(row, 5).Value = b.AccountNumber;
            sheet.Cell(row, 6).Value = b.BranchName;
            sheet.Cell(row, 7).Value = b.BranchCode;
            sheet.Cell(row, 8).Value = b.BranchAddress;
            sheet.Cell(row, 9).Value = b.SwiftCode;
            sheet.Cell(row, 10).Value = b.IntermediaryBankName ?? string.Empty;
            sheet.Cell(row, 11).Value = b.IntermediaryCountry ?? string.Empty;
            sheet.Cell(row, 12).Value = b.IntermediaryBranch ?? string.Empty;
            sheet.Cell(row, 13).Value = b.IntermediarySwiftCode ?? string.Empty;
            row++;
        }

        StyleHeader(sheet, headers.Length);
        sheet.Columns().AdjustToContents();
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(filePath))!);
        workbook.SaveAs(filePath);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public async Task<DictionaryImportResult> ImportBankProfilesAsync(
        string filePath,
        IBankProfileRepository repo,
        CancellationToken ct = default)
    {
        var result = new DictionaryImportResult();
        using var workbook = new XLWorkbook(filePath);
        var sheet = workbook.Worksheets.First();
        var headerRow = sheet.FirstRowUsed() ?? throw new InvalidOperationException("Worksheet is empty.");
        var map = BuildExactHeaderMap(headerRow, new[]
        {
            "ProfileName", "IsDefault", "BankName", "AccountHolderName", "AccountNumber",
            "BranchName", "BranchCode", "BranchAddress", "SwiftCode",
            "IntermediaryBankName", "IntermediaryCountry", "IntermediaryBranch", "IntermediarySwiftCode"
        });
        if (!map.ContainsKey("ProfileName") || !map.ContainsKey("BankName"))
            throw new InvalidOperationException("Required columns: ProfileName, BankName.");

        var all = await repo.GetAllAsync(ct);
        foreach (var row in sheet.RowsUsed().Skip(1))
        {
            ct.ThrowIfCancellationRequested();
            var name = GetCell(row, map, "ProfileName");
            var bank = GetCell(row, map, "BankName");
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(bank))
            {
                result.Skipped++;
                continue;
            }

            var existing = all.FirstOrDefault(b =>
                b.ProfileName.Equals(name, StringComparison.OrdinalIgnoreCase));
            if (existing is null)
            {
                existing = new BankProfile { ProfileName = name };
                await repo.AddAsync(existing, ct);
                all.Add(existing);
                result.Inserted++;
            }
            else
            {
                repo.Update(existing);
                result.Updated++;
            }

            existing.BankName = bank;
            existing.IsDefault = ParseBool(GetCell(row, map, "IsDefault"));
            existing.AccountHolderName = GetCell(row, map, "AccountHolderName") ?? existing.AccountHolderName;
            existing.AccountNumber = GetCell(row, map, "AccountNumber") ?? existing.AccountNumber;
            existing.BranchName = GetCell(row, map, "BranchName") ?? existing.BranchName;
            existing.BranchCode = GetCell(row, map, "BranchCode") ?? existing.BranchCode;
            existing.BranchAddress = GetCell(row, map, "BranchAddress") ?? existing.BranchAddress;
            existing.SwiftCode = GetCell(row, map, "SwiftCode") ?? existing.SwiftCode;
            existing.IntermediaryBankName = GetCell(row, map, "IntermediaryBankName");
            existing.IntermediaryCountry = GetCell(row, map, "IntermediaryCountry");
            existing.IntermediaryBranch = GetCell(row, map, "IntermediaryBranch");
            existing.IntermediarySwiftCode = GetCell(row, map, "IntermediarySwiftCode");
        }

        return result;
    }

    /// <inheritdoc />
    public void WriteChassisLookupSampleTemplate(string filePath)
    {
        using var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add("ChassisLookups");
        for (var i = 0; i < ChassisLookupHeaders.Length; i++)
            sheet.Cell(1, i + 1).Value = ChassisLookupHeaders[i];

        sheet.Cell(2, 1).Value = "KSP130";
        sheet.Cell(2, 2).Value = "TOYOTA";
        sheet.Cell(2, 3).Value = "VITZ";
        sheet.Cell(2, 5).Value = 1000;
        sheet.Cell(2, 6).Value = "PETROL";
        sheet.Cell(2, 7).Value = "HATCHBACK";
        sheet.Cell(2, 8).Value = 5;

        sheet.Cell(3, 1).Value = "KSP130";
        sheet.Cell(3, 2).Value = "TOYOTA";
        sheet.Cell(3, 3).Value = "PROBOX";
        sheet.Cell(3, 5).Value = 1300;
        sheet.Cell(3, 6).Value = "PETROL";
        sheet.Cell(3, 7).Value = "VAN";
        sheet.Cell(3, 8).Value = 5;

        StyleHeader(sheet, ChassisLookupHeaders.Length);
        sheet.Columns().AdjustToContents();
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(filePath))!);
        workbook.SaveAs(filePath);
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

    private static Dictionary<string, int> BuildExactHeaderMap(IXLRow headerRow, IEnumerable<string> expected)
    {
        var set = new HashSet<string>(expected, StringComparer.OrdinalIgnoreCase);
        var map = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        foreach (var cell in headerRow.CellsUsed())
        {
            var header = cell.GetString().Trim();
            if (set.Contains(header))
                map[header] = cell.Address.ColumnNumber;
        }
        return map;
    }

    private static string? GetCell(IXLRow row, Dictionary<string, int> map, string field)
    {
        if (!map.TryGetValue(field, out var col)) return null;
        var value = row.Cell(col).GetFormattedString()?.Trim();
        return string.IsNullOrWhiteSpace(value) ? null : value;
    }

    private static bool ParseBool(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return false;
        return value is "1" or "true" or "TRUE" or "yes" or "YES" or "Y";
    }

    private static void StyleHeader(IXLWorksheet sheet, int columnCount)
    {
        var range = sheet.Range(1, 1, 1, columnCount);
        range.Style.Font.Bold = true;
        range.Style.Fill.BackgroundColor = XLColor.FromHtml("#1F4E79");
        range.Style.Font.FontColor = XLColor.White;
    }
}
