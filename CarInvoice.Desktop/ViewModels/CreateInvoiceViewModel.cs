using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CarInvoice.Core.DTOs;
using CarInvoice.Core.Entities;
using CarInvoice.Core.Enums;
using CarInvoice.Core.Interfaces;
using CarInvoice.Desktop.Services;
using CarInvoice.Desktop.Views;
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
    private readonly IDictionaryLearningService _dictionaryLearning;
    private bool _suppressConsigneeMirror;
    private bool _suppressChassisLookup;

    [ObservableProperty] private string _rawSourceText = string.Empty;
    [ObservableProperty] private string _chassisCode = string.Empty;
    [ObservableProperty] private string _chassisNumber = string.Empty;
    [ObservableProperty] private string _lookupBadge = "Enter chassis to auto-lookup";
    [ObservableProperty] private string? _duplicateWarning;
    [ObservableProperty] private bool _hasDuplicate;

    [ObservableProperty] private string? _stockNumber;
    [ObservableProperty] private string _make = string.Empty;
    [ObservableProperty] private string _model = string.Empty;
    [ObservableProperty] private string? _gradeTrim;
    [ObservableProperty] private int _manufacturingYear = DateTime.Today.Year;
    [ObservableProperty] private int? _engineCapacityCc;
    [ObservableProperty] private string? _engineNumber;
    [ObservableProperty] private string? _exteriorColor;
    [ObservableProperty] private string _fuelType = "PETROL";
    [ObservableProperty] private string _transmission = "Automatic";
    [ObservableProperty] private int _mileage;
    [ObservableProperty] private string _mileageUnit = "KM";
    [ObservableProperty] private int? _doors;
    [ObservableProperty] private string _condition = "USED";
    [ObservableProperty] private string _steering = "RHD";
    [ObservableProperty] private int? _seats;
    [ObservableProperty] private string? _bodyType;
    [ObservableProperty] private int? _unladenWeightKg;
    [ObservableProperty] private string? _auctionGrade;
    [ObservableProperty] private string? _historyNotes;

    [ObservableProperty] private string _invoiceNumber = string.Empty;
    [ObservableProperty] private DateTime _invoiceDate = DateTime.Today;
    [ObservableProperty] private string? _destinationPort;
    [ObservableProperty] private string? _destinationCountry;
    [ObservableProperty] private string _tradeTerm = "C & F";

    [ObservableProperty] private string _customerName = string.Empty;
    [ObservableProperty] private string? _companyName;
    [ObservableProperty] private string _customerPhone = string.Empty;
    [ObservableProperty] private string _customerEmail = string.Empty;
    [ObservableProperty] private string _customerAddress = string.Empty;
    [ObservableProperty] private string? _taxOrNationalId;
    [ObservableProperty] private bool _isConsigneeSameAsCustomer = true;
    [ObservableProperty] private string _consigneeName = string.Empty;
    [ObservableProperty] private string _consigneeAddress = string.Empty;

    [ObservableProperty] private decimal _priceAmount;
    [ObservableProperty] private decimal _taxAmount;
    [ObservableProperty] private decimal _additionalFees;
    [ObservableProperty] private decimal _totalAmount;
    [ObservableProperty] private string _currency = "USD";
    [ObservableProperty] private string _paymentMethod = "Cash";
    [ObservableProperty] private string? _invoiceNotes;
    [ObservableProperty] private string? _lastGeneratedPath;

    [ObservableProperty] private BankProfile? _selectedBankProfile;
    [ObservableProperty] private bool _isClassicOutlookReady;
    [ObservableProperty] private bool _isOfflineOutlookMode = true;
    [ObservableProperty] private string _outlookStatusLabel = "Offline email mode";
    [ObservableProperty] private string _outlookStatusToolTip =
        "New Outlook has no COM inbox sync. Drop a .eml/.msg file or use Paste Email Text.";

    [ObservableProperty] private bool _requiresModelSelection;
    [ObservableProperty] private string? _verificationPrompt;
    [ObservableProperty] private bool _chassisCodeWarning;
    [ObservableProperty] private bool _chassisNumberWarning;
    [ObservableProperty] private bool _customerNameWarning;
    [ObservableProperty] private bool _colorFieldWarning;
    [ObservableProperty] private bool _yearFieldWarning;
    [ObservableProperty] private bool _mileageFieldWarning;
    [ObservableProperty] private bool _priceFieldWarning;
    [ObservableProperty] private bool _showGenerationActions;

    /// <summary>Windows path with real backslashes — bind with PathTextStyle (not Yu Gothic).</summary>
    public string? LastGeneratedPathDisplay =>
        string.IsNullOrWhiteSpace(LastGeneratedPath)
            ? null
            : Path.GetFullPath(LastGeneratedPath);

    public ObservableCollection<string> FuelTypes { get; } = new() { "PETROL", "HYBRID", "DIESEL", "EV" };
    public ObservableCollection<string> Transmissions { get; } = new() { "Automatic", "CVT", "Manual" };
    public ObservableCollection<string> MileageUnits { get; } = new() { "KM", "MILES" };
    public ObservableCollection<string> Conditions { get; } = new() { "USED", "NEW" };
    public ObservableCollection<string> SteeringOptions { get; } = new() { "RHD", "LHD" };
    public ObservableCollection<string> TradeTerms { get; } = new() { "C & F", "FOB", "CIF" };
    public ObservableCollection<string> Currencies { get; } = new(InvoiceCurrencies.Common);
    public ObservableCollection<string> BodyTypes { get; } = new()
    {
        "SEDAN", "HATCHBACK", "SUV", "VAN", "MINI BUS", "WAGON", "TRUCK", "COUPE"
    };
    /// <summary>Filled from JapaneseTermGlossary (Color category) — not a hardcoded palette.</summary>
    public ObservableCollection<string> ColorSuggestions { get; } = new();
    public ObservableCollection<BankProfile> BankProfiles { get; } = new();
    public ObservableCollection<EmailIngestionResult> OutlookMessages { get; } = new();
    public ObservableCollection<ChassisModelOption> ChassisModelOptions { get; } = new();

    /// <summary>Backward-compatible alias used by older bindings.</summary>
    public decimal SalePrice
    {
        get => PriceAmount;
        set => PriceAmount = value;
    }

    public CreateInvoiceViewModel(
        IUnitOfWork uow,
        IChassisParserService chassisParser,
        IJapaneseTranslationService translator,
        IOutlookIngestionService outlook,
        IExcelService excel,
        IWordInvoiceGeneratorService word,
        IAppSettingsService settings,
        OutlookDraftService outlookDraft,
        IDictionaryLearningService dictionaryLearning)
    {
        _uow = uow;
        _chassisParser = chassisParser;
        _translator = translator;
        _outlook = outlook;
        _excel = excel;
        _word = word;
        _settings = settings;
        _outlookDraft = outlookDraft;
        _dictionaryLearning = dictionaryLearning;
        ApplyDefaultCurrencyFromSettings(force: true);
        RefreshOutlookStatus();
        _ = RefreshFromDataStoresAsync();
    }

    /// <summary>Reload currency, bank profiles, and glossary-backed suggestions from DB/settings.</summary>
    public async Task RefreshFromDataStoresAsync()
    {
        await _settings.LoadAsync();
        // Keep the currency the user already picked on this form; only seed if empty
        ApplyDefaultCurrencyFromSettings(force: false);
        await LoadBankProfilesAsync();
        await LoadColorSuggestionsAsync();
    }

    [RelayCommand]
    private async Task LoadBankProfilesAsync()
    {
        BankProfiles.Clear();
        foreach (var profile in await _uow.BankProfiles.GetAllOrderedAsync())
            BankProfiles.Add(profile);
        SelectedBankProfile ??= BankProfiles.FirstOrDefault(b => b.IsDefault) ?? BankProfiles.FirstOrDefault();
    }

    private async Task LoadColorSuggestionsAsync()
    {
        var existing = ColorSuggestions.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var fromGlossary = (await _uow.Glossary.GetAllOrderedAsync())
            .Where(g => string.Equals(g.Category, "Color", StringComparison.OrdinalIgnoreCase))
            .Select(g => g.EnglishTranslation?.Trim())
            .Where(t => !string.IsNullOrWhiteSpace(t))
            .Cast<string>()
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(t => t, StringComparer.OrdinalIgnoreCase);

        foreach (var color in fromGlossary)
        {
            if (existing.Contains(color)) continue;
            ColorSuggestions.Add(color);
            existing.Add(color);
        }

        // Auction-sheet abbreviation often used in source emails
        if (!existing.Contains("CON"))
            ColorSuggestions.Add("CON");
    }

    [RelayCommand]
    private void RefreshOutlookStatus()
    {
        try
        {
            var status = _outlook.CheckOutlookAvailability();
            IsClassicOutlookReady = status == OutlookAvailabilityStatus.ClassicOutlookReady;
            IsOfflineOutlookMode = !IsClassicOutlookReady;
            if (IsClassicOutlookReady)
            {
                OutlookStatusLabel = "Classic Outlook";
                OutlookStatusToolTip = "Classic Outlook is running. Inbox sync and draft-with-attachment are available.";
            }
            else
            {
                OutlookStatusLabel = "Offline email mode";
                OutlookStatusToolTip =
                    "Classic Outlook is not running (or New Outlook is active). Open Classic Outlook to sync the inbox, " +
                    "or drop a .eml/.msg / use Paste Email Text. CarInvoice never launches Outlook on its own.";
            }
        }
        catch
        {
            IsClassicOutlookReady = false;
            IsOfflineOutlookMode = true;
            OutlookStatusLabel = "Offline email mode";
            OutlookStatusToolTip = _outlook.GetUnavailableMessage();
        }
    }

    partial void OnPriceAmountChanged(decimal value)
    {
        RecalcTotal();
        if (value > 0) PriceFieldWarning = false;
        RefreshVerificationPrompt();
    }

    partial void OnTaxAmountChanged(decimal value) => RecalcTotal();
    partial void OnAdditionalFeesChanged(decimal value) => RecalcTotal();
    partial void OnExteriorColorChanged(string? value)
    {
        if (!string.IsNullOrWhiteSpace(value)) ColorFieldWarning = false;
        RefreshVerificationPrompt();
    }

    partial void OnManufacturingYearChanged(int value)
    {
        if (value > 1950) YearFieldWarning = false;
        RefreshVerificationPrompt();
    }

    partial void OnMileageChanged(int value)
    {
        if (value > 0) MileageFieldWarning = false;
        RefreshVerificationPrompt();
    }

    partial void OnChassisCodeChanged(string value)
    {
        if (!string.IsNullOrWhiteSpace(value)) ChassisCodeWarning = false;
        RefreshVerificationPrompt();
        _ = LookupChassisAsync();
    }

    partial void OnChassisNumberChanged(string value)
    {
        if (!string.IsNullOrWhiteSpace(value)) ChassisNumberWarning = false;
        RefreshVerificationPrompt();
        _ = LookupChassisAsync();
    }

    partial void OnCustomerNameChanged(string value)
    {
        if (!string.IsNullOrWhiteSpace(value)) CustomerNameWarning = false;
        RefreshVerificationPrompt();
        if (IsConsigneeSameAsCustomer && !_suppressConsigneeMirror)
            ConsigneeName = value;
    }

    private void RecalcTotal() => TotalAmount = PriceAmount + TaxAmount + AdditionalFees;

    private void RefreshVerificationPrompt()
    {
        if (!ChassisCodeWarning && !ChassisNumberWarning && !CustomerNameWarning
            && !ColorFieldWarning && !YearFieldWarning && !MileageFieldWarning && !PriceFieldWarning)
            VerificationPrompt = null;
    }

    partial void OnCustomerAddressChanged(string value)
    {
        if (IsConsigneeSameAsCustomer && !_suppressConsigneeMirror)
            ConsigneeAddress = value;
    }

    partial void OnIsConsigneeSameAsCustomerChanged(bool value)
    {
        if (!value) return;
        _suppressConsigneeMirror = true;
        ConsigneeName = CustomerName;
        ConsigneeAddress = CustomerAddress;
        _suppressConsigneeMirror = false;
    }

    [RelayCommand]
    private async Task LookupChassisAsync()
    {
        if (_suppressChassisLookup) return;

        var raw = $"{ChassisCode} {ChassisNumber}".Trim();
        if (string.IsNullOrWhiteSpace(raw))
        {
            LookupBadge = "Enter chassis to auto-lookup";
            HasDuplicate = false;
            DuplicateWarning = null;
            RequiresModelSelection = false;
            ChassisModelOptions.Clear();
            return;
        }

        var result = await _chassisParser.ParseAsync(raw);
        _suppressChassisLookup = true;
        try
        {
            if (!string.IsNullOrWhiteSpace(result.ChassisCode) && string.IsNullOrWhiteSpace(ChassisCode))
                ChassisCode = result.ChassisCode;
            if (!string.IsNullOrWhiteSpace(result.ChassisNumber) && string.IsNullOrWhiteSpace(ChassisNumber))
                ChassisNumber = result.ChassisNumber;
        }
        finally { _suppressChassisLookup = false; }

        ChassisModelOptions.Clear();
        RequiresModelSelection = result.RequiresModelSelection;

        if (result.RequiresModelSelection)
        {
            foreach (var match in result.Lookup.Matches)
            {
                ChassisModelOptions.Add(new ChassisModelOption
                {
                    Lookup = match,
                    DisplayLabel = match.DisplayLabel,
                    IsRecommended = result.Lookup.RecommendedMatch is not null
                                    && match.Id == result.Lookup.RecommendedMatch.Id
                });
            }

            LookupBadge = $"{result.Lookup.Matches.Count} models for {result.ChassisCode}";
        }
        else if (!string.IsNullOrWhiteSpace(result.SuggestedMake))
        {
            ApplyLookupDefaults(result.SuggestedMake!, result.SuggestedModel, result.SuggestedEngineCc,
                result.SuggestedFuelType, result.SuggestedBodyType, result.SuggestedDoors,
                result.SuggestedSeats, result.SuggestedEngineNumber);
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
    private void SelectChassisModel(ChassisModelOption? option)
    {
        if (option?.Lookup is null) return;
        var l = option.Lookup;
        ApplyLookupDefaults(l.DefaultMake, l.DefaultModel, l.DefaultEngineCc, l.DefaultFuelType,
            l.DefaultBodyType, l.DefaultDoors, l.DefaultSeats, l.DefaultEngineNumber, force: true);
        RequiresModelSelection = false;
        ChassisModelOptions.Clear();
        LookupBadge = $"✓ {l.DefaultMake} {l.DefaultModel}";
        StatusMessage = $"Selected model: {l.DisplayLabel}";
    }

    private void ApplyLookupDefaults(
        string make,
        string? model,
        int? engineCc,
        string? fuel,
        string? body,
        int? doors,
        int? seats,
        string? engineNo,
        bool force = false)
    {
        if (force || string.IsNullOrWhiteSpace(Make)) Make = make;
        if (force || string.IsNullOrWhiteSpace(Model)) Model = model ?? Model;
        if (force || EngineCapacityCc is null) EngineCapacityCc = engineCc;
        if (force || string.IsNullOrWhiteSpace(FuelType) || FuelType is "Petrol" or "PETROL")
            FuelType = fuel ?? FuelType;
        if (force || string.IsNullOrWhiteSpace(BodyType)) BodyType = body;
        if (force || Doors is null) Doors = doors;
        if (force || Seats is null) Seats = seats;
        if (force || string.IsNullOrWhiteSpace(EngineNumber)) EngineNumber = engineNo;
        RememberSuggestion(BodyTypes, body);
        RememberSuggestion(FuelTypes, fuel);
    }

    private static void RememberSuggestion(ObservableCollection<string> list, string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return;
        if (list.Any(x => x.Equals(value, StringComparison.OrdinalIgnoreCase))) return;
        list.Add(value);
    }

    [RelayCommand]
    private async Task LoadFromOutlookAsync()
    {
        try
        {
            IsBusy = true;
            RefreshOutlookStatus();
            if (!IsClassicOutlookReady)
            {
                StatusMessage = _outlook.GetUnavailableMessage();
                return;
            }

            StatusMessage = "Connecting to Classic Outlook…";
            var messages = await _outlook.FetchFromOutlookAsync(maxCount: 15, unreadOnly: false);
            OutlookMessages.Clear();
            foreach (var m in messages) OutlookMessages.Add(m);
            if (messages.Count == 0)
            {
                StatusMessage = "No messages found in Inbox.";
                return;
            }
            ApplyExtracted(messages[0].Extracted, messages[0].BodyText);
            if (!string.IsNullOrWhiteSpace(messages[0].SenderEmail) && string.IsNullOrWhiteSpace(CustomerEmail))
                CustomerEmail = messages[0].SenderEmail;
            StatusMessage = $"Loaded {messages.Count} Outlook message(s). Showing first.";
        }
        catch (Exception ex)
        {
            RefreshOutlookStatus();
            StatusMessage = ex.Message.Contains("Classic Outlook", StringComparison.OrdinalIgnoreCase)
                ? ex.Message
                : $"Outlook unavailable: {ex.Message}. Use Drop .msg/.eml or Paste Email Text.";
        }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private async Task PasteEmailTextAsync()
    {
        try
        {
            IsBusy = true;
            if (!Clipboard.ContainsText())
            {
                StatusMessage = "Clipboard is empty — copy an email body (Ctrl+A / Ctrl+C) first.";
                return;
            }

            var text = Clipboard.GetText();
            if (string.IsNullOrWhiteSpace(text))
            {
                StatusMessage = "Clipboard text is empty.";
                return;
            }

            var result = await _outlook.ParsePlainTextAsync(text);
            ApplyExtracted(result.Extracted, result.BodyText);
            StatusMessage = "Pasted email text parsed — chassis lookup & JP→EN applied.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Paste parse error: {ex.Message}";
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
            if (!string.IsNullOrWhiteSpace(result.SenderEmail) && string.IsNullOrWhiteSpace(CustomerEmail))
                CustomerEmail = result.SenderEmail;
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
        StockNumber = GradeTrim = EngineNumber = ExteriorColor = AuctionGrade = HistoryNotes = null;
        BodyType = DestinationPort = DestinationCountry = InvoiceNotes = null;
        EngineCapacityCc = Doors = Seats = UnladenWeightKg = null;
        ManufacturingYear = DateTime.Today.Year;
        InvoiceDate = DateTime.Today;
        InvoiceNumber = string.Empty;
        Mileage = 0;
        FuelType = "PETROL";
        Transmission = "Automatic";
        MileageUnit = "KM";
        Condition = "USED";
        Steering = "RHD";
        TradeTerm = "C & F";
        CustomerName = CustomerPhone = CustomerEmail = CustomerAddress = string.Empty;
        CompanyName = TaxOrNationalId = null;
        IsConsigneeSameAsCustomer = true;
        ConsigneeName = ConsigneeAddress = string.Empty;
        PriceAmount = TaxAmount = AdditionalFees = 0;
        ApplyDefaultCurrencyFromSettings(force: true);
        LookupBadge = "Enter chassis to auto-lookup";
        HasDuplicate = false;
        DuplicateWarning = null;
        RequiresModelSelection = false;
        ChassisModelOptions.Clear();
        ClearVerificationWarnings();
        LastGeneratedPath = null;
        ShowGenerationActions = false;
        OnPropertyChanged(nameof(LastGeneratedPathDisplay));
        SelectedBankProfile = BankProfiles.FirstOrDefault(b => b.IsDefault) ?? BankProfiles.FirstOrDefault();
        SetStatus("Form cleared — ready for manual entry.", StatusSeverity.Neutral);
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
            if (string.IsNullOrWhiteSpace(ChassisNumber) || string.IsNullOrWhiteSpace(CustomerName))
            {
                ChassisNumberWarning = string.IsNullOrWhiteSpace(ChassisNumber);
                CustomerNameWarning = string.IsNullOrWhiteSpace(CustomerName);
                SetStatus("Chassis Number and Customer Name are required to save a draft.", StatusSeverity.Warning);
                return;
            }

            var (_, _, invoice) = await PersistAsync(isDraft: true);
            InvoiceNumber = invoice.InvoiceNumber;
            var learned = await LearnDictionariesAsync();
            SetStatus(
                string.IsNullOrWhiteSpace(learned)
                    ? $"Draft saved: {invoice.InvoiceNumber}"
                    : $"Draft saved: {invoice.InvoiceNumber} · {learned}",
                StatusSeverity.Success);
        }
        catch (Exception ex)
        {
            SetStatus($"Save failed: {FormatException(ex)}", StatusSeverity.Error);
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
            if (!ValidateRequiredFieldsForGenerate())
                return;

            if (!ConfirmCriticalFieldsDialog())
            {
                ShowGenerationActions = false;
                SetStatus("Generation cancelled — edit the highlighted details and try again.", StatusSeverity.Warning);
                return;
            }

            IsBusy = true;
            var (customer, vehicle, invoice) = await PersistAsync(isDraft: false);
            InvoiceNumber = invoice.InvoiceNumber;
            var bank = SelectedBankProfile
                       ?? await _uow.BankProfiles.GetDefaultAsync()
                       ?? BankProfiles.FirstOrDefault();
            var request = BuildGenerationRequest(invoice, customer, vehicle, bank);
            var path = await _word.GenerateAsync(request, _settings.TemplatePath, _settings.OutputFolder, openAfter);
            invoice.GeneratedDocPath = path;
            _uow.Invoices.Update(invoice);
            await _uow.SaveChangesAsync();
            LastGeneratedPath = path;
            OnPropertyChanged(nameof(LastGeneratedPathDisplay));
            var learned = await LearnDictionariesAsync();
            var ok = string.IsNullOrWhiteSpace(learned)
                ? "Invoice generated successfully."
                : $"Invoice generated successfully · {learned}";
            SetStatus(ok, StatusSeverity.Success);
            ShowGenerationActions = true;
        }
        catch (Exception ex)
        {
            ShowGenerationActions = false;
            SetStatus($"Generate failed: {FormatException(ex)}", StatusSeverity.Error);
        }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private void OpenGeneratedInvoice()
    {
        if (string.IsNullOrWhiteSpace(LastGeneratedPath) || !File.Exists(LastGeneratedPath))
        {
            SetStatus("Invoice file not found.", StatusSeverity.Warning);
            ShowGenerationActions = false;
            return;
        }

        Process.Start(new ProcessStartInfo
        {
            FileName = LastGeneratedPath,
            UseShellExecute = true
        });
    }

    [RelayCommand]
    private void OpenGeneratedFolder()
    {
        if (string.IsNullOrWhiteSpace(LastGeneratedPath) || !File.Exists(LastGeneratedPath))
        {
            SetStatus("Invoice file not found.", StatusSeverity.Warning);
            ShowGenerationActions = false;
            return;
        }

        Process.Start(new ProcessStartInfo
        {
            FileName = "explorer.exe",
            Arguments = $"/select,\"{LastGeneratedPath}\"",
            UseShellExecute = true
        });
    }

    /// <summary>
    /// Highlights every missing required field and blocks generation until they are filled.
    /// </summary>
    private bool ValidateRequiredFieldsForGenerate()
    {
        ChassisCodeWarning = string.IsNullOrWhiteSpace(ChassisCode);
        ChassisNumberWarning = string.IsNullOrWhiteSpace(ChassisNumber);
        CustomerNameWarning = string.IsNullOrWhiteSpace(CustomerName);
        ColorFieldWarning = string.IsNullOrWhiteSpace(ExteriorColor);
        YearFieldWarning = ManufacturingYear <= 1950;
        MileageFieldWarning = Mileage <= 0;
        PriceFieldWarning = PriceAmount <= 0;

        var missing = new List<string>();
        if (ChassisCodeWarning) missing.Add("Chassis Code");
        if (ChassisNumberWarning) missing.Add("Chassis Number");
        if (CustomerNameWarning) missing.Add("Customer Name");
        if (ColorFieldWarning) missing.Add("Color");
        if (YearFieldWarning) missing.Add("Year");
        if (MileageFieldWarning) missing.Add("Odometer");
        if (PriceFieldWarning) missing.Add("Price");

        if (missing.Count > 0)
        {
            VerificationPrompt =
                $"Please fill the highlighted required fields before generating: {string.Join(", ", missing)}.";
            ShowGenerationActions = false;
            SetStatus(VerificationPrompt, StatusSeverity.Warning);
            return false;
        }

        ClearVerificationWarnings();
        return true;
    }

    private void ClearVerificationWarnings()
    {
        ChassisCodeWarning = ChassisNumberWarning = CustomerNameWarning = false;
        ColorFieldWarning = YearFieldWarning = MileageFieldWarning = PriceFieldWarning = false;
        VerificationPrompt = null;
    }

    private bool ConfirmCriticalFieldsDialog()
    {
        Currency = AppSettingsService.NormalizeCurrency(Currency);
        EnsureCurrencyInList(Currency);
        var priceText =
            $"{Currency} {PriceAmount.ToString("N0", CultureInfo.InvariantCulture)}";

        var dialog = new InvoiceVerificationDialog(
            chassis: $"{ChassisCode}-{ChassisNumber}".Trim('-'),
            makerModel: $"{Make} {Model}".Trim(),
            color: ExteriorColor ?? string.Empty,
            year: ManufacturingYear.ToString(CultureInfo.InvariantCulture),
            odometer: $"{Mileage.ToString("N0", CultureInfo.InvariantCulture)} {MileageUnit}",
            price: priceText,
            tradeTerm: TradeTerm);

        dialog.Owner = Application.Current.MainWindow;
        return dialog.ShowDialog() == true && dialog.Confirmed;
    }

    /// <summary>
    /// Settings currency is the default for new/cleared invoices only.
    /// Per-invoice choice on this form is never overwritten by settings.
    /// </summary>
    private void ApplyDefaultCurrencyFromSettings(bool force)
    {
        var defaults = AppSettingsService.NormalizeCurrency(_settings.Currency);
        EnsureCurrencyInList(defaults);
        if (force || string.IsNullOrWhiteSpace(Currency))
            Currency = defaults;
        else
        {
            Currency = AppSettingsService.NormalizeCurrency(Currency);
            EnsureCurrencyInList(Currency);
        }
    }

    private void EnsureCurrencyInList(string code)
    {
        if (string.IsNullOrWhiteSpace(code)) return;
        if (!Currencies.Any(c => string.Equals(c, code, StringComparison.OrdinalIgnoreCase)))
            Currencies.Insert(0, code);
    }

    private async Task<string?> LearnDictionariesAsync()
    {
        try
        {
            var result = await _dictionaryLearning.LearnFromInvoiceAsync(
                ChassisCode, Make, Model, EngineCapacityCc, BodyType, Doors, Seats, FuelType,
                EngineNumber, ChassisNumber, ExteriorColor, HistoryNotes, RawSourceText);
            RememberSuggestion(ColorSuggestions, ExteriorColor);
            RememberSuggestion(BodyTypes, BodyType);
            RememberSuggestion(FuelTypes, FuelType);
            return result.StatusMessage;
        }
        catch
        {
            return null;
        }
    }

    [RelayCommand]
    private async Task CreateOutlookDraftAsync()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(LastGeneratedPath) || !File.Exists(LastGeneratedPath))
            {
                await GenerateWordCoreAsync(false);
                if (string.IsNullOrWhiteSpace(LastGeneratedPath) || !File.Exists(LastGeneratedPath))
                    return;
            }

            IsBusy = true;
            RefreshOutlookStatus();
            var subject = $"Invoice {System.IO.Path.GetFileNameWithoutExtension(LastGeneratedPath)} — {Make} {Model}".Trim(' ', '—');
            var summary =
                $"Dear {CustomerName},\n\n" +
                $"Please find your vehicle export invoice for {Make} {Model}".Trim() +
                (string.IsNullOrWhiteSpace(ChassisNumber) ? "" : $" (chassis {ChassisCode}-{ChassisNumber})") +
                $".\nTotal: {TotalAmount:N0} {Currency}.\n\nRegards";

            var outcome = _outlookDraft.CreateDraftOrFallback(
                CustomerEmail,
                subject,
                summary,
                LastGeneratedPath);

            StatusMessage = outcome.Message;
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
                if (!string.IsNullOrWhiteSpace(result.SenderEmail) && string.IsNullOrWhiteSpace(CustomerEmail))
                    CustomerEmail = result.SenderEmail;
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

        if (!string.IsNullOrWhiteSpace(f.StockNumber)) StockNumber = f.StockNumber;
        if (!string.IsNullOrWhiteSpace(f.ChassisCode)) ChassisCode = f.ChassisCode!;
        if (!string.IsNullOrWhiteSpace(f.ChassisNumber)) ChassisNumber = f.ChassisNumber!;
        if (!string.IsNullOrWhiteSpace(f.Make)) Make = f.Make!;
        if (!string.IsNullOrWhiteSpace(f.Model)) Model = f.Model!;
        if (f.Year is > 1900) ManufacturingYear = f.Year.Value;
        if (f.Mileage is > 0) Mileage = f.Mileage.Value;
        if (!string.IsNullOrWhiteSpace(f.Color)) ExteriorColor = f.Color;
        if (f.Price is > 0) PriceAmount = f.Price.Value;
        if (f.EngineCc is > 0) EngineCapacityCc = f.EngineCc;
        if (!string.IsNullOrWhiteSpace(f.EngineNumber)) EngineNumber = f.EngineNumber;
        if (f.Doors is > 0) Doors = f.Doors;
        if (f.Seats is > 0) Seats = f.Seats;
        if (f.UnladenWeightKg is > 0) UnladenWeightKg = f.UnladenWeightKg;
        if (!string.IsNullOrWhiteSpace(f.Condition)) Condition = f.Condition!;
        if (!string.IsNullOrWhiteSpace(f.Steering)) Steering = f.Steering!;
        if (!string.IsNullOrWhiteSpace(f.BodyType)) BodyType = f.BodyType;
        if (!string.IsNullOrWhiteSpace(f.DestinationPort)) DestinationPort = f.DestinationPort;
        if (!string.IsNullOrWhiteSpace(f.DestinationCountry)) DestinationCountry = f.DestinationCountry;
        if (!string.IsNullOrWhiteSpace(f.TradeTerm)) TradeTerm = f.TradeTerm!;
        if (!string.IsNullOrWhiteSpace(f.CustomerName)) CustomerName = f.CustomerName!;
        if (!string.IsNullOrWhiteSpace(f.CustomerAddress)) CustomerAddress = f.CustomerAddress!;
        if (!string.IsNullOrWhiteSpace(f.AuctionGrade)) AuctionGrade = f.AuctionGrade;
        if (!string.IsNullOrWhiteSpace(f.FuelType)) FuelType = f.FuelType!.ToUpperInvariant();
        if (!string.IsNullOrWhiteSpace(f.Transmission)) Transmission = f.Transmission!;
        if (!string.IsNullOrWhiteSpace(f.GradeTrim)) GradeTrim = f.GradeTrim;
        HistoryNotes = f.TranslatedRemarks ?? f.JapaneseRemarks ?? HistoryNotes;

        var consigneeMatchesCustomer =
            string.IsNullOrWhiteSpace(f.ConsigneeName)
            || string.Equals(f.ConsigneeName, f.CustomerName, StringComparison.OrdinalIgnoreCase);
        IsConsigneeSameAsCustomer = consigneeMatchesCustomer;
        if (!string.IsNullOrWhiteSpace(f.ConsigneeName))
            ConsigneeName = f.ConsigneeName!;
        else if (IsConsigneeSameAsCustomer)
            ConsigneeName = CustomerName;
        if (!string.IsNullOrWhiteSpace(f.ConsigneeAddress))
            ConsigneeAddress = f.ConsigneeAddress!;
        else if (IsConsigneeSameAsCustomer)
            ConsigneeAddress = CustomerAddress;
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

        vehicle.StockNumber = StockNumber;
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
        vehicle.Doors = Doors;
        vehicle.Condition = Condition;
        vehicle.Steering = Steering;
        vehicle.Seats = Seats;
        vehicle.BodyType = BodyType;
        vehicle.UnladenWeightKg = UnladenWeightKg;
        vehicle.AuctionGrade = AuctionGrade;
        vehicle.HistoryNotes = HistoryNotes;
        vehicle.UpdatedAt = DateTime.UtcNow;

        await _uow.SaveChangesAsync();

        if (IsConsigneeSameAsCustomer)
        {
            ConsigneeName = CustomerName;
            ConsigneeAddress = CustomerAddress;
        }

        // Prefer user-entered number only when it is unused; otherwise allocate the next unique number.
        // (Re-generating with a previous InvoiceNumber still filled used to hit UNIQUE and fail save.)
        Currency = AppSettingsService.NormalizeCurrency(Currency);
        EnsureCurrencyInList(Currency);
        var number = await ResolveInvoiceNumberAsync();
        var invoice = new Invoice
        {
            InvoiceNumber = number,
            InvoiceDate = InvoiceDate == default ? DateTime.Today : InvoiceDate,
            CustomerId = customer.Id,
            VehicleId = vehicle.Id,
            DestinationPort = DestinationPort?.ToUpperInvariant(),
            DestinationCountry = DestinationCountry?.ToUpperInvariant(),
            IsConsigneeSameAsCustomer = IsConsigneeSameAsCustomer,
            ConsigneeName = ConsigneeName,
            ConsigneeAddress = ConsigneeAddress,
            TradeTerm = TradeTerm,
            PriceAmount = PriceAmount,
            SalePrice = PriceAmount,
            TaxAmount = TaxAmount,
            AdditionalFees = AdditionalFees,
            TotalAmount = TotalAmount,
            Currency = Currency,
            PaymentMethod = PaymentMethod,
            Notes = InvoiceNotes,
            // Only set FK — SelectedBankProfile is AsNoTracking; assigning the navigation
            // makes EF try to INSERT a duplicate BankProfiles row (PK violation).
            BankProfileId = SelectedBankProfile?.Id,
            IsDraft = isDraft
        };
        await _uow.Invoices.AddAsync(invoice);
        await _uow.SaveChangesAsync();
        invoice.Customer = customer;
        invoice.Vehicle = vehicle;
        return (customer, vehicle, invoice);
    }

    private async Task<string> ResolveInvoiceNumberAsync()
    {
        var requested = InvoiceNumber?.Trim();
        if (!string.IsNullOrWhiteSpace(requested))
        {
            var existing = await _uow.Invoices.GetByNumberAsync(requested);
            if (existing is null)
                return requested;
        }

        return await _uow.Invoices.GenerateNextInvoiceNumberAsync();
    }

    private static string FormatException(Exception ex)
    {
        var parts = new List<string> { ex.Message };
        for (var inner = ex.InnerException; inner is not null; inner = inner.InnerException)
            parts.Add(inner.Message);
        return string.Join(" → ", parts.Distinct());
    }

    private static InvoiceGenerationRequest BuildGenerationRequest(
        Invoice invoice,
        Customer customer,
        Vehicle vehicle,
        BankProfile? bank)
        => new()
        {
            InvoiceNumber = invoice.InvoiceNumber,
            InvoiceDate = invoice.InvoiceDate,
            DestinationPort = invoice.DestinationPort,
            DestinationCountry = invoice.DestinationCountry,
            CustomerName = customer.FullName,
            CustomerPhone = customer.Phone,
            CustomerEmail = customer.Email,
            CustomerAddress = customer.Address,
            CompanyName = customer.CompanyName,
            ConsigneeName = invoice.ConsigneeName ?? customer.FullName,
            ConsigneeAddress = invoice.ConsigneeAddress ?? customer.Address,
            StockNumber = vehicle.StockNumber,
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
            Doors = vehicle.Doors,
            Condition = vehicle.Condition,
            Steering = vehicle.Steering,
            Seats = vehicle.Seats,
            BodyType = vehicle.BodyType,
            FuelType = vehicle.FuelType,
            UnladenWeightKg = vehicle.UnladenWeightKg,
            Transmission = vehicle.Transmission,
            AuctionGrade = vehicle.AuctionGrade,
            HistoryNotes = vehicle.HistoryNotes,
            TradeTerm = invoice.TradeTerm,
            PriceAmount = invoice.PriceAmount > 0 ? invoice.PriceAmount : invoice.SalePrice,
            SalePrice = invoice.SalePrice,
            TaxAmount = invoice.TaxAmount,
            AdditionalFees = invoice.AdditionalFees,
            TotalAmount = invoice.TotalAmount,
            Currency = invoice.Currency,
            PaymentMethod = invoice.PaymentMethod,
            Notes = invoice.Notes,
            BankName = bank?.BankName,
            AccountHolderName = bank?.AccountHolderName,
            AccountNumber = bank?.AccountNumber,
            BranchName = bank?.BranchName,
            BranchCode = bank?.BranchCode,
            BranchAddress = bank?.BranchAddress,
            SwiftCode = bank?.SwiftCode,
            IntermediaryBankName = bank?.IntermediaryBankName,
            IntermediaryCountry = bank?.IntermediaryCountry,
            IntermediaryBranch = bank?.IntermediaryBranch,
            IntermediarySwiftCode = bank?.IntermediarySwiftCode
        };
}
