using System.Windows;
using System.Windows.Media;
using CarInvoice.Core.Interfaces;
using CarInvoice.Core.Services;
using CarInvoice.Data;
using CarInvoice.Desktop.Behaviors;
using CarInvoice.Desktop.Services;
using CarInvoice.Desktop.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Wpf.Ui;
using Wpf.Ui.Appearance;

namespace CarInvoice.Desktop;

public partial class App : Application
{
    /// <summary>Ai-iro — Primary CTA. Must override Windows system accent (often purple).</summary>
    private static readonly Color AccentAiIro = (Color)ColorConverter.ConvertFromString("#2B4C7E")!;
    private static readonly Color AccentAiIroHover = (Color)ColorConverter.ConvertFromString("#1F385E")!;
    private static readonly Color AccentRuriIro = (Color)ColorConverter.ConvertFromString("#1D5B9E")!;

    private static readonly IHost Host = Microsoft.Extensions.Hosting.Host
        .CreateDefaultBuilder()
        .ConfigureServices((_, services) =>
        {
            var dbPath = DatabaseInitializer.GetDefaultDatabasePath();
            services.AddCarInvoiceData(dbPath);

            services.AddSingleton<IAppSettingsService, AppSettingsService>();
            services.AddSingleton<OutlookDraftService>();

            services.AddSingleton<IChassisParserService, ChassisParserService>();
            services.AddSingleton<IJapaneseTranslationService, JapaneseTranslationService>();
            services.AddSingleton<IOutlookIngestionService, OutlookIngestionService>();
            services.AddSingleton<IExcelService, ExcelService>();
            services.AddSingleton<IWordInvoiceGeneratorService, WordInvoiceGeneratorService>();
            services.AddSingleton<IDictionaryLearningService, DictionaryLearningService>();

            services.AddSingleton<CreateInvoiceViewModel>();
            services.AddSingleton<VehiclesViewModel>();
            services.AddSingleton<InvoicesViewModel>();
            services.AddSingleton<SettingsViewModel>();
            services.AddSingleton<MainWindowViewModel>();
            services.AddSingleton<MainWindow>();

            services.AddSingleton<IThemeService, ThemeService>();
            services.AddSingleton<ITaskBarService, TaskBarService>();
        })
        .Build();

    public static IServiceProvider Services => Host.Services;

    /// <summary>
    /// Forces Shaken &amp; Washi Ai-iro onto Wpf.Ui Primary buttons so they ignore the Windows accent color.
    /// </summary>
    private static void ApplyShakenWashiAccent()
    {
        // Explicit primary/secondary/tertiary — do not call ApplySystemAccent().
        ApplicationAccentColorManager.Apply(AccentAiIro, AccentAiIro, AccentAiIroHover, AccentRuriIro);

        var theme = Host.Services.GetRequiredService<IThemeService>();
        theme.SetTheme(ApplicationTheme.Light);
        theme.SetAccent(AccentAiIro);
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        try
        {
            // Before any window: ComboBox wheel must not change selection when closed,
            // and must not be stolen by page ScrollViewers when the dropdown is open.
            ComboBoxWheelFix.Register();

            await Host.StartAsync();
            ApplyShakenWashiAccent();

            await DatabaseInitializer.InitializeAsync(Host.Services, createBackup: true);

            var word = Host.Services.GetRequiredService<IWordInvoiceGeneratorService>();
            var settings = Host.Services.GetRequiredService<IAppSettingsService>();
            await settings.LoadAsync();
            settings.Currency = AppSettingsService.NormalizeCurrency(settings.Currency);

            var appData = System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CarInvoice");
            var templatesDir = System.IO.Path.Combine(appData, "Templates");
            word.EnsureDefaultTemplate(System.IO.Path.Combine(AppContext.BaseDirectory, "Templates"));
            var template = word.EnsureDefaultTemplate(templatesDir);
            if (string.IsNullOrWhiteSpace(settings.TemplatePath) || !File.Exists(settings.TemplatePath))
                settings.TemplatePath = template;

            Directory.CreateDirectory(settings.OutputFolder);
            Directory.CreateDirectory(settings.BackupFolder);
            await settings.SaveAsync();

            var translator = Host.Services.GetRequiredService<IJapaneseTranslationService>();
            await translator.ReloadAsync();

            var main = Host.Services.GetRequiredService<MainWindow>();
            main.Show();

            base.OnStartup(e);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"CarInvoice failed to start:\n\n{ex.Message}\n\n{ex.InnerException?.Message}",
                "Startup Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            Shutdown(-1);
        }
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        await Host.StopAsync();
        Host.Dispose();
        base.OnExit(e);
    }
}
