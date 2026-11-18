using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CarInvoice.Core.Entities;
using CarInvoice.Core.Interfaces;
using Microsoft.Win32;

namespace CarInvoice.Desktop.ViewModels;

public partial class VehiclesViewModel : ViewModelBase
{
    private readonly IUnitOfWork _uow;
    private readonly IExcelService _excel;

    [ObservableProperty] private string _searchText = string.Empty;
    [ObservableProperty] private Vehicle? _selectedVehicle;
    [ObservableProperty] private bool _isDetailsOpen;

    public ObservableCollection<Vehicle> Vehicles { get; } = new();

    public VehiclesViewModel(IUnitOfWork uow, IExcelService excel)
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
            var list = await _uow.Vehicles.SearchAsync(SearchText);
            Vehicles.Clear();
            foreach (var v in list) Vehicles.Add(v);
            StatusMessage = $"{Vehicles.Count} vehicle(s)";
        }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private async Task SearchAsync() => await LoadAsync();

    [RelayCommand]
    private void ShowDetails(Vehicle? vehicle)
    {
        SelectedVehicle = vehicle;
        IsDetailsOpen = vehicle is not null;
    }

    [RelayCommand]
    private async Task ExportAsync()
    {
        var dlg = new SaveFileDialog
        {
            Filter = "Excel (*.xlsx)|*.xlsx",
            FileName = $"vehicles_{DateTime.Now:yyyyMMdd}.xlsx"
        };
        if (dlg.ShowDialog() != true) return;
        await _excel.ExportVehiclesAsync(Vehicles, dlg.FileName);
        StatusMessage = $"Exported to {dlg.FileName}";
    }
}
