using System.Windows.Controls;
using System.Windows.Input;
using CarInvoice.Core.Entities;
using CarInvoice.Desktop.ViewModels;

namespace CarInvoice.Desktop.Views;

public partial class InvoicesView : UserControl
{
    public InvoicesView()
    {
        InitializeComponent();
    }

    private void OnRowDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (DataContext is InvoicesViewModel vm && sender is DataGrid grid && grid.SelectedItem is Invoice inv)
            vm.OpenDocumentCommand.Execute(inv);
    }
}
