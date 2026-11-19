using System.Windows;

namespace CarInvoice.Desktop.Views;

public partial class InvoiceVerificationDialog : Window
{
    public bool Confirmed { get; private set; }

    public InvoiceVerificationDialog(
        string chassis,
        string makerModel,
        string color,
        string year,
        string odometer,
        string price,
        string tradeTerm)
    {
        InitializeComponent();
        ChassisText.Text = chassis;
        MakerText.Text = makerModel;
        ColorText.Text = color;
        YearText.Text = year;
        OdometerText.Text = odometer;
        PriceText.Text = price;
        TradeTermText.Text = $"Trade term: {tradeTerm}";
    }

    private void OnConfirmClick(object sender, RoutedEventArgs e)
    {
        Confirmed = true;
        DialogResult = true;
        Close();
    }

    private void OnEditClick(object sender, RoutedEventArgs e)
    {
        Confirmed = false;
        DialogResult = false;
        Close();
    }
}
