using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CarInvoice.Core.Entities;
using CarInvoice.Core.Interfaces;
using CarInvoice.Data;
using Microsoft.Win32;

namespace CarInvoice.Desktop.ViewModels;

public partial class SettingsViewModel : ViewModelBase
{
    private readonly IUnitOfWork _uow;
    private readonly IAppSettingsService _settings;
    private readonly IJapaneseTranslationService _translator;
    private readonly IWordInvoiceGeneratorService _word;

    [ObservableProperty] private string _templatePath = string.Empty;
    [ObservableProperty] private string _outputFolder = string.Empty;
    [ObservableProperty] private string _backupFolder = string.Empty;
    [ObservableProperty] private string _databasePath = string.Empty;
    [ObservableProperty] private string _currency = "PKR";

    [ObservableProperty] private string _newChassisCode = string.Empty;
    [ObservableProperty] private string _newMake = string.Empty;
    [ObservableProperty] private string _newModel = string.Empty;
    [ObservableProperty] private int? _newEngineCc;
    [ObservableProperty] private string _newFuel = "Petrol";

    [ObservableProperty] private string _newJapanese = string.Empty;
    [ObservableProperty] private string _newEnglish = string.Empty;
    [ObservableProperty] private string _newCategory = "Grade/Spec";

    public ObservableCollection<ChassisCodeLookup> ChassisLookups { get; } = new();
    public ObservableCollection<JapaneseTermGlossary> Glossary { get; } = new();
    public ObservableCollection<string> Categories { get; } = new()
    {
        "Color", "Condition/History", "Grade/Spec", "Fuel/Transmission"
    };

    public SettingsViewModel(
        IUnitOfWork uow,
        IAppSettingsService settings,
        IJapaneseTranslationService translator,
        IWordInvoiceGeneratorService word)
    {
        _uow = uow;
        _settings = settings;
        _translator = translator;
        _word = word;
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
            Currency = _settings.Currency;

            ChassisLookups.Clear();
            foreach (var c in (await _uow.ChassisLookups.GetAllAsync()).OrderBy(c => c.ChassisCode))
                ChassisLookups.Add(c);

            Glossary.Clear();
            foreach (var g in await _uow.Glossary.GetAllOrderedAsync())
                Glossary.Add(g);

            StatusMessage = $"{ChassisLookups.Count} chassis codes, {Glossary.Count} glossary terms";
        }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private async Task SaveSettingsAsync()
    {
        _settings.TemplatePath = TemplatePath;
        _settings.OutputFolder = OutputFolder;
        _settings.BackupFolder = BackupFolder;
        _settings.Currency = Currency;
        await _settings.SaveAsync();
        Directory.CreateDirectory(OutputFolder);
        Directory.CreateDirectory(BackupFolder);
        _word.EnsureDefaultTemplate(System.IO.Path.GetDirectoryName(TemplatePath) ?? "Templates");
        StatusMessage = "Settings saved.";
    }

    [RelayCommand]
    private void BrowseTemplate()
    {
        var dlg = new OpenFileDialog { Filter = "Word (*.docx)|*.docx" };
        if (dlg.ShowDialog() == true) TemplatePath = dlg.FileName;
    }

    [RelayCommand]
    private void BrowseOutput()
    {
        // Simple folder picker via SaveFileDialog workaround for zero extra deps
        var dlg = new SaveFileDialog
        {
            Title = "Select output folder (type any name)",
            FileName = "select_folder",
            Filter = "Folder|*.folder"
        };
        if (dlg.ShowDialog() == true)
            OutputFolder = System.IO.Path.GetDirectoryName(dlg.FileName) ?? OutputFolder;
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
