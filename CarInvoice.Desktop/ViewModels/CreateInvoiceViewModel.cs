using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CarInvoice.Core.DTOs;
using CarInvoice.Core.Entities;
using CarInvoice.Core.Interfaces;
using CarInvoice.Desktop.Services;
using Microsoft.Win32;

namespace CarInvoice.Desktop.ViewModels;

public partial class CreateInvoiceViewModel : ViewModelBase
{
    private readonly IUnitOfWork _uow;
    private readonly IChassisParserService _chassisParser;
    private readonly IJapaneseTranslationService _translator;
    private readonly IOutlookIngestionService _outlook;
    private readonly IExcelService _excel;
    private readonly IWordInvoiceGeneratorService _word;
    private readonly IAppSettingsService _settings;
    private readonly OutlookDraftService _outlookDraft;

    [ObservableProperty] private string _rawSourceText = string.Empty;
    [ObservableProperty] private string _chassisCode = string.Empty;
    [ObservableProperty] private string _chassisNumber = string.Empty;
    [ObservableProperty] private string _lookupBadge = "Enter chassis to auto-lookup";
    [ObservableProperty] private string? _duplicateWarning;
    [ObservableProperty] private bool _hasDuplicate;

    [ObservableProperty] private string _make = string.Empty;
    [ObservableProperty] private string _model = string.Empty;
    [ObservableProperty] private string? _gradeTrim;
    [ObservableProperty] private int _manufacturingYear = DateTime.Today.Year;
    [ObservableProperty] private int? _engineCapacityCc;
    [ObservableProperty] private string? _engineNumber;
    [ObservableProperty] private string? _exteriorColor;
    [ObservableProperty] private string _fuelType = "Petrol";
    [ObservableProperty] private string _transmission = "Automatic";
    [ObservableProperty] private int _mileage;
    [ObservableProperty] private string _mileageUnit = "km";
    [ObservableProperty] private string? _auctionGrade;
    [ObservableProperty] private string? _historyNotes;

    [ObservableProperty] private string _customerName = string.Empty;
    [ObservableProperty] private string? _companyName;
    [ObservableProperty] private string _customerPhone = string.Empty;
    [ObservableProperty] private string _customerEmail = string.Empty;
    [ObservableProperty] private string _customerAddress = string.Empty;
    [ObservableProperty] private string? _taxOrNationalId;

    [ObservableProperty] private decimal _salePrice;
    [ObservableProperty] private decimal _taxAmount;
    [ObservableProperty] private decimal _additionalFees;
    [ObservableProperty] private decimal _totalAmount;
    [ObservableProperty] private string _currency = "PKR";
    [ObservableProperty] private string _paymentMethod = "Cash";
    [ObservableProperty] private string? _invoiceNotes;
    [ObservableProperty] private string? _lastGeneratedPath;

    public ObservableCollection<string> FuelTypes { get; } = new() { "Petrol", "Hybrid", "Diesel", "EV" };
    public ObservableCollection<string> Transmissions { get; } = new() { "Automatic", "CVT", "Manual" };
    public ObservableCollection<string> MileageUnits { get; } = new() { "km", "miles" };
    public ObservableCollection<EmailIngestionResult> OutlookMessages { get; } = new();

    public CreateInvoiceViewModel(
        IUnitOfWork uow,
        IChassisParserService chassisParser,
        IJapaneseTranslationService translator,
        IOutlookIngestionService outlook,
        IExcelService excel,
        IWordInvoiceGeneratorService word,
        IAppSettingsService settings,
        OutlookDraftService outlookDraft)
    {
        _uow = uow;
        _chassisParser = chassisParser;
        _translator = translator;
        _outlook = outlook;
        _excel = excel;
        _word = word;
        _settings = settings;
        _outlookDraft = outlookDraft;
        Currency = settings.Currency;
    }

    partial void OnSalePriceChanged(decimal value) => RecalcTotal();
    partial void OnTaxAmountChanged(decimal value) => RecalcTotal();
    partial void OnAdditionalFeesChanged(decimal value) => RecalcTotal();

    private void RecalcTotal() => TotalAmount = SalePrice + TaxAmount + AdditionalFees;

    partial void OnChassisCodeChanged(string value) => _ = LookupChassisAsync();
    partial void OnChassisNumberChanged(string value) => _ = LookupChassisAsync();

    [RelayCommand]
    private async Task LookupChassisAsync()
    {
        var raw = $"{ChassisCode} {ChassisNumber}".Trim();
        if (string.IsNullOrWhiteSpace(raw))
        {
            LookupBadge = "Enter chassis to auto-lookup";
            HasDuplicate = false;
            DuplicateWarning = null;
            return;
        }

        var result = await _chassisParser.ParseAsync(raw);
        if (!string.IsNullOrWhiteSpace(result.ChassisCode) && string.IsNullOrWhiteSpace(ChassisCode))
            ChassisCode = result.ChassisCode;
        if (!string.IsNullOrWhiteSpace(result.ChassisNumber) && string.IsNullOrWhiteSpace(ChassisNumber))
            ChassisNumber = result.ChassisNumber;

        if (!string.IsNullOrWhiteSpace(result.SuggestedMake))
        {
            if (string.IsNullOrWhiteSpace(Make)) Make = result.SuggestedMake!;
            if (string.IsNullOrWhiteSpace(Model)) Model = result.SuggestedModel ?? Model;
            if (EngineCapacityCc is null) EngineCapacityCc = result.SuggestedEngineCc;
            if (string.IsNullOrWhiteSpace(FuelType) || FuelType == "Petrol")
                FuelType = result.SuggestedFuelType ?? FuelType;
            LookupBadge = $"✓ {result.SuggestedMake} {result.SuggestedModel}";
        }
        else
        {
            LookupBadge = "No chassis code match in lookup table";
        }

        HasDuplicate = result.IsDuplicate;
        DuplicateWarning = result.DuplicateWarning;
        if (result.ExistingVehicle is not null && string.IsNullOrWhiteSpace(HistoryNotes))
            HistoryNotes = result.ExistingVehicle.HistoryNotes;
    }

    [RelayCommand]
    private async Task LoadFromOutlookAsync()
    {
        try
        {
            IsBusy = true;
            StatusMessage = "Connecting to Outlook…";
            var messages = await _outlook.FetchFromOutlookAsync(maxCount: 15, unreadOnly: false);
            OutlookMessages.Clear();
            foreach (var m in messages) OutlookMessages.Add(m);
            if (messages.Count == 0)
            {
                StatusMessage = "No messages found in Inbox.";
                return;
            }
            ApplyExtracted(messages[0].Extracted, messages[0].BodyText);
            StatusMessage = $"Loaded {messages.Count} Outlook message(s). Showing first.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Outlook error: {ex.Message}";
        }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private async Task DropMessageFileAsync()
    {
        var dlg = new OpenFileDialog
        {
            Filter = "Outlook / Email (*.msg;*.eml)|*.msg;*.eml|All files (*.*)|*.*",
            Title = "Select .msg or .eml file"
        };
        if (dlg.ShowDialog() != true) return;

        try
        {
            IsBusy = true;
            var result = await _outlook.ParseMessageFileAsync(dlg.FileName);
            ApplyExtracted(result.Extracted, result.BodyText);
            StatusMessage = $"Parsed {System.IO.Path.GetFileName(dlg.FileName)}";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Parse error: {ex.Message}";
        }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private async Task ImportExcelRowAsync()
    {
        var dlg = new OpenFileDialog
        {
            Filter = "Excel (*.xlsx)|*.xlsx",
            Title = "Import vehicle stock sheet"
        };
        if (dlg.ShowDialog() != true) return;

        try
        {
            IsBusy = true;
            var rows = await _excel.ImportVehiclesAsync(dlg.FileName);
            if (rows.Count == 0)
            {
                StatusMessage = "No vehicle rows found.";
                return;
            }
            ApplyExtracted(rows[0], string.Join(Environment.NewLine, rows.Select(r =>
                $"{r.ChassisCode}-{r.ChassisNumber} {r.Make} {r.Model} {r.Color} {r.Mileage}")));
            StatusMessage = $"Imported first of {rows.Count} row(s) from Excel.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Excel import error: {ex.Message}";
        }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private void ClearForm()
    {
        RawSourceText = string.Empty;
        ChassisCode = ChassisNumber = Make = Model = string.Empty;
        GradeTrim = EngineNumber = ExteriorColor = AuctionGrade = HistoryNotes = null;
        EngineCapacityCc = null;
        ManufacturingYear = DateTime.Today.Year;
        Mileage = 0;
        FuelType = "Petrol";
        Transmission = "Automatic";
        CustomerName = CustomerPhone = CustomerEmail = CustomerAddress = string.Empty;
        CompanyName = TaxOrNationalId = InvoiceNotes = null;
        SalePrice = TaxAmount = AdditionalFees = 0;
        LookupBadge = "Enter chassis to auto-lookup";
        HasDuplicate = false;
        DuplicateWarning = null;
        LastGeneratedPath = null;
        StatusMessage = "Form cleared — ready for manual entry.";
    }

    [RelayCommand]
    private async Task TranslateJapaneseAsync()
    {
        if (string.IsNullOrWhiteSpace(RawSourceText)) return;
        IsBusy = true;
        try
        {
            RawSourceText = await _translator.TranslateAsync(RawSourceText, keepOriginalInParentheses: true);
            if (!string.IsNullOrWhiteSpace(HistoryNotes))
                HistoryNotes = await _translator.TranslateAsync(HistoryNotes, true);
            if (!string.IsNullOrWhiteSpace(ExteriorColor))
                ExteriorColor = await _translator.TranslateAsync(ExteriorColor, false);
            StatusMessage = "Japanese terms translated.";
        }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private async Task SaveDraftAsync()
    {
        try
        {
            IsBusy = true;
            var (customer, vehicle, invoice) = await PersistAsync(isDraft: true);
            StatusMessage = $"Draft saved: {invoice.InvoiceNumber}";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Save failed: {ex.Message}";
        }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private async Task GenerateWordDocAsync() => await GenerateWordCoreAsync(openAfter: false);

    [RelayCommand]
    private async Task GenerateAndOpenAsync() => await GenerateWordCoreAsync(openAfter: true);

    private async Task GenerateWordCoreAsync(bool openAfter)
    {
        try
        {
            IsBusy = true;
            var (customer, vehicle, invoice) = await PersistAsync(isDraft: false);
            var request = BuildGenerationRequest(invoice, customer, vehicle);
            var path = await _word.GenerateAsync(request, _settings.TemplatePath, _settings.OutputFolder, openAfter);
            invoice.GeneratedDocPath = path;
            _uow.Invoices.Update(invoice);
            await _uow.SaveChangesAsync();
            LastGeneratedPath = path;
            StatusMessage = $"Invoice generated: {path}";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Generate failed: {ex.Message}";
        }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private async Task CreateOutlookDraftAsync()
    {
        try
        {
            IsBusy = true;
            if (string.IsNullOrWhiteSpace(LastGeneratedPath) || !File.Exists(LastGeneratedPath))
                await GenerateWordCoreAsync(false);

            var ok = _outlookDraft.TryCreateDraft(
                CustomerEmail,
                $"Invoice {System.IO.Path.GetFileNameWithoutExtension(LastGeneratedPath)} — {Make} {Model}",
                $"Dear {CustomerName},\n\nPlease find attached your vehicle sale invoice.\n\nRegards",
                LastGeneratedPath,
                out var error);

            StatusMessage = ok ? "Outlook draft opened." : $"Outlook draft failed: {error}";
        }
        catch (Exception ex)
        {
            StatusMessage = ex.Message;
        }
        finally { IsBusy = false; }
    }

    public void ApplyDroppedFile(string filePath)
    {
        _ = ApplyDroppedFileAsync(filePath);
    }

    private async Task ApplyDroppedFileAsync(string filePath)
    {
        try
        {
            IsBusy = true;
            var ext = System.IO.Path.GetExtension(filePath).ToLowerInvariant();
            if (ext is ".msg" or ".eml")
            {
                var result = await _outlook.ParseMessageFileAsync(filePath);
                ApplyExtracted(result.Extracted, result.BodyText);
                StatusMessage = $"Dropped file parsed: {System.IO.Path.GetFileName(filePath)}";
            }
            else if (ext is ".xlsx")
            {
                var rows = await _excel.ImportVehiclesAsync(filePath);
                if (rows.Count > 0)
                    ApplyExtracted(rows[0], RawSourceText);
                StatusMessage = $"Dropped Excel imported ({rows.Count} rows).";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = ex.Message;
        }
        finally { IsBusy = false; }
    }

    private void ApplyExtracted(ExtractedVehicleFields f, string? rawText)
    {
        if (!string.IsNullOrWhiteSpace(rawText))
            RawSourceText = rawText;

        if (!string.IsNullOrWhiteSpace(f.ChassisCode)) ChassisCode = f.ChassisCode!;
        if (!string.IsNullOrWhiteSpace(f.ChassisNumber)) ChassisNumber = f.ChassisNumber!;
        if (!string.IsNullOrWhiteSpace(f.Make)) Make = f.Make!;
        if (!string.IsNullOrWhiteSpace(f.Model)) Model = f.Model!;
        if (f.Year is > 1900) ManufacturingYear = f.Year.Value;
        if (f.Mileage is > 0) Mileage = f.Mileage.Value;
        if (!string.IsNullOrWhiteSpace(f.Color)) ExteriorColor = f.Color;
        if (f.Price is > 0) SalePrice = f.Price.Value;
        if (!string.IsNullOrWhiteSpace(f.CustomerName)) CustomerName = f.CustomerName!;
        if (!string.IsNullOrWhiteSpace(f.AuctionGrade)) AuctionGrade = f.AuctionGrade;
        if (!string.IsNullOrWhiteSpace(f.FuelType)) FuelType = f.FuelType!;
        if (!string.IsNullOrWhiteSpace(f.Transmission)) Transmission = f.Transmission!;
        if (!string.IsNullOrWhiteSpace(f.GradeTrim)) GradeTrim = f.GradeTrim;
        HistoryNotes = f.TranslatedRemarks ?? f.JapaneseRemarks ?? HistoryNotes;
    }

    private async Task<(Customer customer, Vehicle vehicle, Invoice invoice)> PersistAsync(bool isDraft)
    {
        if (string.IsNullOrWhiteSpace(ChassisNumber))
            throw new InvalidOperationException("Chassis number is required.");
        if (string.IsNullOrWhiteSpace(CustomerName))
            throw new InvalidOperationException("Customer name is required.");

        var normalized = _chassisParser.Normalize(ChassisCode, ChassisNumber);

        var customer = await _uow.Customers.FindByNameOrPhoneAsync(CustomerName)
                       ?? await _uow.Customers.FindByNameOrPhoneAsync(CustomerPhone);
        if (customer is null)
        {
            customer = new Customer
            {
                FullName = CustomerName,
                CompanyName = CompanyName,
                Phone = CustomerPhone,
                Email = CustomerEmail,
                Address = CustomerAddress,
                TaxOrNationalId = TaxOrNationalId
            };
            await _uow.Customers.AddAsync(customer);
        }
        else
        {
            customer.Phone = string.IsNullOrWhiteSpace(CustomerPhone) ? customer.Phone : CustomerPhone;
            customer.Email = string.IsNullOrWhiteSpace(CustomerEmail) ? customer.Email : CustomerEmail;
            customer.Address = string.IsNullOrWhiteSpace(CustomerAddress) ? customer.Address : CustomerAddress;
            customer.CompanyName = CompanyName ?? customer.CompanyName;
            _uow.Customers.Update(customer);
        }

        await _uow.SaveChangesAsync();

        var vehicle = await _uow.Vehicles.FindByChassisNumberAsync(ChassisNumber);
        if (vehicle is null)
        {
            vehicle = new Vehicle();
            await _uow.Vehicles.AddAsync(vehicle);
        }
        else
        {
            _uow.Vehicles.Update(vehicle);
        }

        vehicle.ChassisCode = ChassisCode.ToUpperInvariant();
        vehicle.ChassisNumber = ChassisNumber.Trim();
        vehicle.FullChassisNormalized = normalized;
        vehicle.Make = Make;
        vehicle.Model = Model;
        vehicle.GradeTrim = GradeTrim;
        vehicle.ManufacturingYear = ManufacturingYear;
        vehicle.EngineCapacityCc = EngineCapacityCc;
        vehicle.EngineNumber = EngineNumber;
        vehicle.ExteriorColor = ExteriorColor;
        vehicle.FuelType = FuelType;
        vehicle.Transmission = Transmission;
        vehicle.Mileage = Mileage;
        vehicle.MileageUnit = MileageUnit;
        vehicle.AuctionGrade = AuctionGrade;
        vehicle.HistoryNotes = HistoryNotes;
        vehicle.UpdatedAt = DateTime.UtcNow;

        await _uow.SaveChangesAsync();

        var number = await _uow.Invoices.GenerateNextInvoiceNumberAsync();
        var invoice = new Invoice
        {
            InvoiceNumber = number,
            InvoiceDate = DateTime.Today,
            CustomerId = customer.Id,
            VehicleId = vehicle.Id,
            SalePrice = SalePrice,
            TaxAmount = TaxAmount,
            AdditionalFees = AdditionalFees,
            TotalAmount = TotalAmount,
            Currency = Currency,
            PaymentMethod = PaymentMethod,
            Notes = InvoiceNotes,
            IsDraft = isDraft,
            Customer = customer,
            Vehicle = vehicle
        };
        await _uow.Invoices.AddAsync(invoice);
        await _uow.SaveChangesAsync();
        return (customer, vehicle, invoice);
    }

    private InvoiceGenerationRequest BuildGenerationRequest(Invoice invoice, Customer customer, Vehicle vehicle)
        => new()
        {
            InvoiceNumber = invoice.InvoiceNumber,
            InvoiceDate = invoice.InvoiceDate,
            CustomerName = customer.FullName,
            CustomerPhone = customer.Phone,
            CustomerEmail = customer.Email,
            CustomerAddress = customer.Address,
            CompanyName = customer.CompanyName,
            Make = vehicle.Make,
            Model = vehicle.Model,
            GradeTrim = vehicle.GradeTrim,
            Year = vehicle.ManufacturingYear,
            ChassisCode = vehicle.ChassisCode,
            ChassisNumber = vehicle.ChassisNumber,
            EngineCc = vehicle.EngineCapacityCc,
            EngineNumber = vehicle.EngineNumber,
            Color = vehicle.ExteriorColor,
            Mileage = vehicle.Mileage,
            MileageUnit = vehicle.MileageUnit,
            FuelType = vehicle.FuelType,
            Transmission = vehicle.Transmission,
            AuctionGrade = vehicle.AuctionGrade,
            HistoryNotes = vehicle.HistoryNotes,
            SalePrice = invoice.SalePrice,
            TaxAmount = invoice.TaxAmount,
            AdditionalFees = invoice.AdditionalFees,
            TotalAmount = invoice.TotalAmount,
            Currency = invoice.Currency,
            PaymentMethod = invoice.PaymentMethod,
            Notes = invoice.Notes
        };
}
