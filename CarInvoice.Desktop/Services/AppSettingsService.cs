using CarInvoice.Core.Entities;
using CarInvoice.Core.Interfaces;
using CarInvoice.Data;
using Microsoft.EntityFrameworkCore;

namespace CarInvoice.Desktop.Services;

public sealed class AppSettingsService : IAppSettingsService
{
    private readonly AppDbContext _db;

    public AppSettingsService(AppDbContext db)
    {
        _db = db;
        DatabasePath = DatabaseInitializer.GetDefaultDatabasePath();
        var appDir = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CarInvoice");
        TemplatePath = System.IO.Path.Combine(AppContext.BaseDirectory, "Templates", "DefaultInvoiceTemplate.docx");
        OutputFolder = System.IO.Path.Combine(appDir, "Invoices");
        BackupFolder = DatabaseInitializer.GetDefaultBackupFolder();
        Currency = "PKR";
    }

    public string DatabasePath { get; private set; }
    public string TemplatePath { get; set; }
    public string OutputFolder { get; set; }
    public string BackupFolder { get; set; }
    public string Currency { get; set; }

    public async Task LoadAsync(CancellationToken ct = default)
    {
        var settings = await _db.AppSettings.AsNoTracking().ToListAsync(ct);
        TemplatePath = Get(settings, nameof(TemplatePath), TemplatePath);
        OutputFolder = Get(settings, nameof(OutputFolder), OutputFolder);
        BackupFolder = Get(settings, nameof(BackupFolder), BackupFolder);
        Currency = Get(settings, nameof(Currency), Currency);
        DatabasePath = Get(settings, nameof(DatabasePath), DatabasePath);
    }

    public async Task SaveAsync(CancellationToken ct = default)
    {
        await UpsertAsync(nameof(TemplatePath), TemplatePath, ct);
        await UpsertAsync(nameof(OutputFolder), OutputFolder, ct);
        await UpsertAsync(nameof(BackupFolder), BackupFolder, ct);
        await UpsertAsync(nameof(Currency), Currency, ct);
        await UpsertAsync(nameof(DatabasePath), DatabasePath, ct);
        await _db.SaveChangesAsync(ct);
    }

    private static string Get(List<AppSetting> settings, string key, string fallback)
        => settings.FirstOrDefault(s => s.Key == key)?.Value ?? fallback;

    private async Task UpsertAsync(string key, string value, CancellationToken ct)
    {
        var existing = await _db.AppSettings.FirstOrDefaultAsync(s => s.Key == key, ct);
        if (existing is null)
            await _db.AppSettings.AddAsync(new AppSetting { Key = key, Value = value }, ct);
        else
            existing.Value = value;
    }
}
