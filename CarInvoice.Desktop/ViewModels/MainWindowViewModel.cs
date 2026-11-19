using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CarInvoice.Desktop.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    private readonly CreateInvoiceViewModel _createInvoice;
    private readonly VehiclesViewModel _vehicles;
    private readonly InvoicesViewModel _invoices;
    private readonly SettingsViewModel _settings;

    [ObservableProperty]
    private ViewModelBase _currentPage;

    [ObservableProperty]
    private string _title = "CarInvoice — Second-Hand Car Invoices";

    public MainWindowViewModel(
        CreateInvoiceViewModel createInvoice,
        VehiclesViewModel vehicles,
        InvoicesViewModel invoices,
        SettingsViewModel settings)
    {
        _createInvoice = createInvoice;
        _vehicles = vehicles;
        _invoices = invoices;
        _settings = settings;
        _currentPage = createInvoice;
    }

    [RelayCommand]
    private void Navigate(string page)
    {
        CurrentPage = page switch
        {
            "Vehicles" => _vehicles,
            "Invoices" => _invoices,
            "Settings" => _settings,
            _ => _createInvoice
        };

        if (CurrentPage is CreateInvoiceViewModel c) _ = c.RefreshFromDataStoresAsync();
        if (CurrentPage is VehiclesViewModel v) _ = v.LoadAsync();
        if (CurrentPage is InvoicesViewModel i) _ = i.LoadAsync();
        if (CurrentPage is SettingsViewModel s) _ = s.LoadAsync();
    }
}
