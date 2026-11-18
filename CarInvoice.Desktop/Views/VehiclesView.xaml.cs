using System.Windows.Controls;
using System.Windows.Input;
using CarInvoice.Core.Entities;
using CarInvoice.Desktop.ViewModels;

namespace CarInvoice.Desktop.Views;

public partial class VehiclesView : UserControl
{
    public VehiclesView()
    {
        InitializeComponent();
    }

    private void OnRowDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (DataContext is VehiclesViewModel vm && sender is DataGrid grid && grid.SelectedItem is Vehicle v)
            vm.ShowDetailsCommand.Execute(v);
    }
}
