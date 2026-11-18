using System.Collections.ObjectModel;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CarInvoice.Core.Entities;
using CarInvoice.Core.Interfaces;
using Microsoft.Win32;

namespace CarInvoice.Desktop.ViewModels;

public partial class InvoicesViewModel : ViewModelBase
{
    private readonly IUnitOfWork _uow;
    private readonly IExcelService _excel;

    [ObservableProperty] private string _searchText = string.Empty;
    [ObservableProperty] private Invoice? _selectedInvoice;

    public ObservableCollection<Invoice> Invoices { get; } = new();

    public InvoicesViewModel(IUnitOfWork uow, IExcelService excel)
    {
        _uow = uow;
        _excel = excel;
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        IsBusy = true;
        try
        {
            var list = await _uow.Invoices.SearchAsync(SearchText);
            Invoices.Clear();
            foreach (var i in list) Invoices.Add(i);
            StatusMessage = $"{Invoices.Count} invoice(s)";
        }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private async Task SearchAsync() => await LoadAsync();

    [RelayCommand]
    private void OpenDocument(Invoice? invoice)
    {
        if (invoice?.GeneratedDocPath is null || !File.Exists(invoice.GeneratedDocPath))
        {
            StatusMessage = "Document not found.";
            return;
        }
        Process.Start(new ProcessStartInfo { FileName = invoice.GeneratedDocPath, UseShellExecute = true });
    }

    [RelayCommand]
    private async Task ExportLedgerAsync()
    {
        var dlg = new SaveFileDialog
        {
            Filter = "Excel (*.xlsx)|*.xlsx",
            FileName = $"invoice_ledger_{DateTime.Now:yyyyMMdd}.xlsx"
        };
        if (dlg.ShowDialog() != true) return;
        await _excel.ExportInvoicesAsync(Invoices, dlg.FileName);
        StatusMessage = $"Ledger exported to {dlg.FileName}";
    }
}
