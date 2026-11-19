using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CarInvoice.Core.Entities;
using CarInvoice.Core.Interfaces;
using CarInvoice.Data;
using CarInvoice.Desktop.Services;
using Microsoft.Win32;

namespace CarInvoice.Desktop.ViewModels;

public partial class SettingsViewModel : ViewModelBase
{
    private readonly IUnitOfWork _uow;
    private readonly IAppSettingsService _settings;
    private readonly IJapaneseTranslationService _translator;
    private readonly IWordInvoiceGeneratorService _word;
    private readonly IExcelService _excel;

    [ObservableProperty] private string _templatePath = string.Empty;
    [ObservableProperty] private string _outputFolder = string.Empty;
    [ObservableProperty] private string _backupFolder = string.Empty;
    [ObservableProperty] private string _databasePath = string.Empty;
    [ObservableProperty] private string _currency = "USD";

    /// <summary>Default currency for new invoices (Create Invoice can override per invoice).</summary>
    public ObservableCollection<string> Currencies { get; } = new(InvoiceCurrencies.Common);

    [ObservableProperty] private string _newChassisCode = string.Empty;
    [ObservableProperty] private string _newMake = string.Empty;
    [ObservableProperty] private string _newModel = string.Empty;
    [ObservableProperty] private int? _newEngineCc;
    [ObservableProperty] private string _newFuel = "PETROL";

    [ObservableProperty] private string _newJapanese = string.Empty;
    [ObservableProperty] private string _newEnglish = string.Empty;
    [ObservableProperty] private string _newCategory = "Grade/Spec";

    // Bank form fields — filled from BankProfiles table via LoadBankIntoForm / NewBankProfile
    [ObservableProperty] private string _bankProfileName = string.Empty;
    [ObservableProperty] private string _bankName = string.Empty;
    [ObservableProperty] private string _accountHolderName = string.Empty;
    [ObservableProperty] private string _accountNumber = string.Empty;
    [ObservableProperty] private string _branchName = string.Empty;
    [ObservableProperty] private string _branchCode = string.Empty;
    [ObservableProperty] private string _branchAddress = string.Empty;
    [ObservableProperty] private string _swiftCode = string.Empty;
    [ObservableProperty] private string _intermediaryBankName = string.Empty;
    [ObservableProperty] private string _intermediaryCountry = string.Empty;
    [ObservableProperty] private string _intermediaryBranch = string.Empty;
    [ObservableProperty] private string _intermediarySwiftCode = string.Empty;
    [ObservableProperty] private bool _bankIsDefault;
    [ObservableProperty] private BankProfile? _selectedBankProfile;

    public ObservableCollection<ChassisCodeLookup> ChassisLookups { get; } = new();
    public ObservableCollection<JapaneseTermGlossary> Glossary { get; } = new();
    public ObservableCollection<BankProfile> BankProfiles { get; } = new();
    public ObservableCollection<string> Categories { get; } = new()
    {
        "Color", "Condition/History", "Grade/Spec", "Fuel/Transmission"
    };

    public SettingsViewModel(
        IUnitOfWork uow,
        IAppSettingsService settings,
        IJapaneseTranslationService translator,
        IWordInvoiceGeneratorService word,
        IExcelService excel)
    {
        _uow = uow;
        _settings = settings;
        _translator = translator;
        _word = word;
        _excel = excel;
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        IsBusy = true;
        try
        {
            await _settings.LoadAsync();
            TemplatePath = _settings.TemplatePath;
            OutputFolder = _settings.OutputFolder;
            BackupFolder = _settings.BackupFolder;
            DatabasePath = _settings.DatabasePath;
            Currency = AppSettingsService.NormalizeCurrency(_settings.Currency);

            ChassisLookups.Clear();
            foreach (var c in (await _uow.ChassisLookups.GetAllAsync()).OrderBy(c => c.ChassisCode))
                ChassisLookups.Add(c);

            Glossary.Clear();
            foreach (var g in await _uow.Glossary.GetAllOrderedAsync())
                Glossary.Add(g);

            await ReloadBanksAsync();

            StatusMessage = $"{ChassisLookups.Count} chassis codes, {Glossary.Count} glossary terms, {BankProfiles.Count} bank profiles";
        }
        finally { IsBusy = false; }
    }

    private async Task ReloadBanksAsync()
    {
        BankProfiles.Clear();
        foreach (var b in await _uow.BankProfiles.GetAllOrderedAsync())
            BankProfiles.Add(b);
        SelectedBankProfile = BankProfiles.FirstOrDefault(b => b.IsDefault) ?? BankProfiles.FirstOrDefault();
        if (SelectedBankProfile is not null)
            LoadBankIntoForm(SelectedBankProfile);
    }

    partial void OnSelectedBankProfileChanged(BankProfile? value)
    {
        if (value is not null)
            LoadBankIntoForm(value);
    }

    private void LoadBankIntoForm(BankProfile b)
    {
        BankProfileName = b.ProfileName;
        BankIsDefault = b.IsDefault;
        BankName = b.BankName;
        AccountHolderName = b.AccountHolderName;
        AccountNumber = b.AccountNumber;
        BranchName = b.BranchName;
        BranchCode = b.BranchCode;
        BranchAddress = b.BranchAddress;
        SwiftCode = b.SwiftCode;
        IntermediaryBankName = b.IntermediaryBankName ?? string.Empty;
        IntermediaryCountry = b.IntermediaryCountry ?? string.Empty;
        IntermediaryBranch = b.IntermediaryBranch ?? string.Empty;
        IntermediarySwiftCode = b.IntermediarySwiftCode ?? string.Empty;
    }

    [RelayCommand]
    private async Task SaveSettingsAsync()
    {
        NormalizePathFields();
        await PersistPathsAsync();
        _word.EnsureDefaultTemplate(System.IO.Path.GetDirectoryName(TemplatePath) ?? "Templates");
        StatusMessage = "Settings saved.";
    }

    [RelayCommand]
    private async Task BrowseTemplateAsync()
    {
        var dlg = new OpenFileDialog
        {
            Title = "Select Word invoice template",
            Filter = "Word (*.docx)|*.docx",
            CheckFileExists = true
        };

        var currentDir = SafeDirectoryName(TemplatePath);
        if (currentDir is not null)
            dlg.InitialDirectory = currentDir;

        if (dlg.ShowDialog() != true)
            return;

        TemplatePath = Path.GetFullPath(dlg.FileName);
        await PersistPathsAsync();
        StatusMessage = "Template path updated.";
    }

    [RelayCommand]
    private async Task BrowseOutputAsync()
    {
        var folder = PickFolder("Select invoice output folder", OutputFolder);
        if (folder is null)
            return;

        OutputFolder = folder;
        await PersistPathsAsync();
        StatusMessage = "Output folder updated.";
    }

    [RelayCommand]
    private async Task BrowseBackupAsync()
    {
        var folder = PickFolder("Select backup folder", BackupFolder);
        if (folder is null)
            return;

        BackupFolder = folder;
        await PersistPathsAsync();
        StatusMessage = "Backup folder updated.";
    }

    private static string? PickFolder(string title, string currentPath)
    {
        var dlg = new OpenFolderDialog
        {
            Title = title,
            Multiselect = false
        };

        var initial = SafeExistingDirectory(currentPath);
        if (initial is not null)
            dlg.InitialDirectory = initial;

        return dlg.ShowDialog() == true && !string.IsNullOrWhiteSpace(dlg.FolderName)
            ? Path.GetFullPath(dlg.FolderName)
            : null;
    }

    private async Task PersistPathsAsync()
    {
        NormalizePathFields();
        _settings.TemplatePath = TemplatePath;
        _settings.OutputFolder = OutputFolder;
        _settings.BackupFolder = BackupFolder;
        _settings.Currency = AppSettingsService.NormalizeCurrency(Currency);
        Currency = _settings.Currency;
        await _settings.SaveAsync();

        if (!string.IsNullOrWhiteSpace(OutputFolder))
            Directory.CreateDirectory(OutputFolder);
        if (!string.IsNullOrWhiteSpace(BackupFolder))
            Directory.CreateDirectory(BackupFolder);
    }

    private void NormalizePathFields()
    {
        TemplatePath = NormalizePath(TemplatePath) ?? TemplatePath?.Trim() ?? string.Empty;
        OutputFolder = NormalizePath(OutputFolder) ?? OutputFolder?.Trim() ?? string.Empty;
        BackupFolder = NormalizePath(BackupFolder) ?? BackupFolder?.Trim() ?? string.Empty;
    }

    private static string? NormalizePath(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return null;
        try
        {
            return Path.GetFullPath(path.Trim());
        }
        catch
        {
            return path.Trim();
        }
    }

    private static string? SafeDirectoryName(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return null;
        try
        {
            var dir = Path.GetDirectoryName(path);
            return Directory.Exists(dir) ? dir : null;
        }
        catch
        {
            return null;
        }
    }

    private static string? SafeExistingDirectory(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return null;
        try
        {
            var full = Path.GetFullPath(path.Trim());
            return Directory.Exists(full) ? full : SafeDirectoryName(full);
        }
        catch
        {
            return null;
        }
    }

    [RelayCommand]
    private async Task AddChassisAsync()
    {
        if (string.IsNullOrWhiteSpace(NewChassisCode)) return;
        var entity = new ChassisCodeLookup
        {
            ChassisCode = NewChassisCode.Trim().ToUpperInvariant(),
            DefaultMake = NewMake,
            DefaultModel = NewModel,
            DefaultEngineCc = NewEngineCc,
            DefaultFuelType = NewFuel
        };
        await _uow.ChassisLookups.AddAsync(entity);
        await _uow.SaveChangesAsync();
        ChassisLookups.Add(entity);
        NewChassisCode = NewMake = NewModel = string.Empty;
        NewEngineCc = null;
        StatusMessage = "Chassis lookup added.";
    }

    [RelayCommand]
    private async Task DeleteChassisAsync(ChassisCodeLookup? item)
    {
        if (item is null) return;
        _uow.ChassisLookups.Remove(item);
        await _uow.SaveChangesAsync();
        ChassisLookups.Remove(item);
    }

    [RelayCommand]
    private async Task ExportChassisExcelAsync()
    {
        var dlg = new SaveFileDialog
        {
            Filter = "Excel (*.xlsx)|*.xlsx",
            FileName = "ChassisCodeLookups.xlsx"
        };
        if (dlg.ShowDialog() != true) return;
        await _excel.ExportChassisLookupsAsync(ChassisLookups, dlg.FileName);
        StatusMessage = $"Exported {ChassisLookups.Count} chassis lookups.";
    }

    [RelayCommand]
    private async Task ImportChassisExcelAsync()
    {
        var dlg = new OpenFileDialog { Filter = "Excel (*.xlsx)|*.xlsx" };
        if (dlg.ShowDialog() != true) return;
        try
        {
            IsBusy = true;
            var result = await _excel.ImportChassisLookupsAsync(dlg.FileName, _uow.ChassisLookups);
            await _uow.SaveChangesAsync();
            await LoadAsync();
            StatusMessage = result.Summary + " (chassis lookups).";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Chassis import failed: {ex.Message}";
        }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private void DownloadChassisSample()
    {
        var dlg = new SaveFileDialog
        {
            Filter = "Excel (*.xlsx)|*.xlsx",
            FileName = "ChassisLookup_Import_Template.xlsx"
        };
        if (dlg.ShowDialog() != true) return;
        _excel.WriteChassisLookupSampleTemplate(dlg.FileName);
        StatusMessage = $"Sample template saved: {dlg.FileName}";
    }

    [RelayCommand]
    private async Task ExportGlossaryExcelAsync()
    {
        var dlg = new SaveFileDialog
        {
            Filter = "Excel (*.xlsx)|*.xlsx",
            FileName = "JapaneseGlossary.xlsx"
        };
        if (dlg.ShowDialog() != true) return;
        await _excel.ExportGlossaryAsync(Glossary, dlg.FileName);
        StatusMessage = $"Exported {Glossary.Count} glossary terms.";
    }

    [RelayCommand]
    private async Task ImportGlossaryExcelAsync()
    {
        var dlg = new OpenFileDialog { Filter = "Excel (*.xlsx)|*.xlsx" };
        if (dlg.ShowDialog() != true) return;
        try
        {
            IsBusy = true;
            var result = await _excel.ImportGlossaryAsync(dlg.FileName, _uow.Glossary);
            await _uow.SaveChangesAsync();
            await _translator.ReloadAsync();
            await LoadAsync();
            StatusMessage = result.Summary + " (glossary).";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Glossary import failed: {ex.Message}";
        }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private async Task ExportBankExcelAsync()
    {
        var dlg = new SaveFileDialog
        {
            Filter = "Excel (*.xlsx)|*.xlsx",
            FileName = "BankProfiles.xlsx"
        };
        if (dlg.ShowDialog() != true) return;
        await _excel.ExportBankProfilesAsync(BankProfiles, dlg.FileName);
        StatusMessage = $"Exported {BankProfiles.Count} bank profiles.";
    }

    [RelayCommand]
    private async Task ImportBankExcelAsync()
    {
        var dlg = new OpenFileDialog { Filter = "Excel (*.xlsx)|*.xlsx" };
        if (dlg.ShowDialog() != true) return;
        try
        {
            IsBusy = true;
            var result = await _excel.ImportBankProfilesAsync(dlg.FileName, _uow.BankProfiles);
            await _uow.SaveChangesAsync();
            await ReloadBanksAsync();
            StatusMessage = result.Summary + " (bank profiles).";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Bank import failed: {ex.Message}";
        }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private async Task SaveBankProfileAsync()
    {
        if (string.IsNullOrWhiteSpace(BankProfileName) || string.IsNullOrWhiteSpace(BankName))
        {
            StatusMessage = "Bank profile name and bank name are required.";
            return;
        }

        if (BankIsDefault)
        {
            foreach (var existing in await _uow.BankProfiles.GetAllAsync())
            {
                if (!existing.IsDefault) continue;
                existing.IsDefault = false;
                _uow.BankProfiles.Update(existing);
            }
        }

        BankProfile entity;
        if (SelectedBankProfile is null)
        {
            entity = new BankProfile();
            await _uow.BankProfiles.AddAsync(entity);
        }
        else
        {
            entity = SelectedBankProfile;
            _uow.BankProfiles.Update(entity);
        }

        entity.ProfileName = BankProfileName.Trim();
        entity.IsDefault = BankIsDefault;
        entity.BankName = BankName.Trim();
        entity.AccountHolderName = AccountHolderName.Trim();
        entity.AccountNumber = AccountNumber.Trim();
        entity.BranchName = BranchName.Trim();
        entity.BranchCode = BranchCode.Trim();
        entity.BranchAddress = BranchAddress.Trim();
        entity.SwiftCode = SwiftCode.Trim();
        entity.IntermediaryBankName = IntermediaryBankName.Trim();
        entity.IntermediaryCountry = IntermediaryCountry.Trim();
        entity.IntermediaryBranch = IntermediaryBranch.Trim();
        entity.IntermediarySwiftCode = IntermediarySwiftCode.Trim();

        await _uow.SaveChangesAsync();
        await ReloadBanksAsync();
        SelectedBankProfile = BankProfiles.FirstOrDefault(b => b.Id == entity.Id);
        StatusMessage = $"Bank profile saved: {entity.ProfileName}";
    }

    [RelayCommand]
    private void NewBankProfile()
    {
        SelectedBankProfile = null;
        BankProfileName = string.Empty;
        BankIsDefault = false;
        BankName = AccountHolderName = AccountNumber = BranchName = BranchCode = string.Empty;
        BranchAddress = SwiftCode = string.Empty;
        IntermediaryBankName = IntermediaryCountry = IntermediaryBranch = IntermediarySwiftCode = string.Empty;
        StatusMessage = "New bank profile — fill fields and Save Bank Profile.";
    }

    [RelayCommand]
    private async Task DeleteBankProfileAsync(BankProfile? item)
    {
        item ??= SelectedBankProfile;
        if (item is null) return;
        _uow.BankProfiles.Remove(item);
        await _uow.SaveChangesAsync();
        await ReloadBanksAsync();
        StatusMessage = "Bank profile deleted.";
    }

    [RelayCommand]
    private async Task AddGlossaryAsync()
    {
        if (string.IsNullOrWhiteSpace(NewJapanese) || string.IsNullOrWhiteSpace(NewEnglish)) return;
        await _translator.AddOrUpdateTermAsync(NewJapanese.Trim(), NewEnglish.Trim(), NewCategory);
        Glossary.Clear();
        foreach (var g in await _uow.Glossary.GetAllOrderedAsync())
            Glossary.Add(g);
        NewJapanese = NewEnglish = string.Empty;
        StatusMessage = "Glossary term saved.";
    }

    [RelayCommand]
    private async Task DeleteGlossaryAsync(JapaneseTermGlossary? item)
    {
        if (item is null) return;
        _uow.Glossary.Remove(item);
        await _uow.SaveChangesAsync();
        Glossary.Remove(item);
        await _translator.ReloadAsync();
    }

    [RelayCommand]
    private void BackupNow()
    {
        DatabaseInitializer.BackupDatabase(DatabasePath, BackupFolder);
        StatusMessage = $"Backup created in {BackupFolder}";
    }
}
