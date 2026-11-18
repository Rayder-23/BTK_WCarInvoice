using System.Windows;
using Wpf.Ui.Controls;
using CarInvoice.Desktop.ViewModels;

namespace CarInvoice.Desktop;

public partial class MainWindow : FluentWindow
{
    private bool _fittedToScreen;

    public MainWindow(MainWindowViewModel viewModel)
    {
        DataContext = viewModel;
        InitializeComponent();
        FitToWorkingArea();
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        // Re-apply after DPI / chrome metrics are known.
        FitToWorkingArea();
    }

    /// <summary>
    /// Sizes and centers the window within the current monitor's working area
    /// (excludes taskbar), clamping mins so small / high-DPI screens still fit.
    /// </summary>
    private void FitToWorkingArea()
    {
        var work = SystemParameters.WorkArea;
        if (work.Width <= 0 || work.Height <= 0) return;

        const double preferredWidth = 1280;
        const double preferredHeight = 800;
        const double margin = 0.04; // keep a small gap from screen edges

        var maxWidth = Math.Max(640, work.Width * (1 - margin));
        var maxHeight = Math.Max(480, work.Height * (1 - margin));

        MinWidth = Math.Min(800, maxWidth);
        MinHeight = Math.Min(560, maxHeight);

        Width = Math.Min(preferredWidth, maxWidth);
        Height = Math.Min(preferredHeight, maxHeight);

        // Never exceed working area even if mins were larger than the screen.
        if (Width > work.Width) Width = work.Width;
        if (Height > work.Height) Height = work.Height;

        Left = work.Left + (work.Width - Width) / 2;
        Top = work.Top + (work.Height - Height) / 2;

        if (Left < work.Left) Left = work.Left;
        if (Top < work.Top) Top = work.Top;
        if (Left + Width > work.Right) Left = Math.Max(work.Left, work.Right - Width);
        if (Top + Height > work.Bottom) Top = Math.Max(work.Top, work.Bottom - Height);

        // Maximize automatically on very small screens where preferred size barely fits.
        if (!_fittedToScreen && (work.Width < 1100 || work.Height < 700))
        {
            WindowState = WindowState.Maximized;
        }

        _fittedToScreen = true;
    }
}
