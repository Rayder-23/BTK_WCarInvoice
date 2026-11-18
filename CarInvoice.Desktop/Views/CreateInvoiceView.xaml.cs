using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CarInvoice.Desktop.ViewModels;

namespace CarInvoice.Desktop.Views;

public partial class CreateInvoiceView : UserControl
{
    public CreateInvoiceView()
    {
        InitializeComponent();
    }

    private void OnDragOver(object sender, DragEventArgs e)
    {
        e.Effects = e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.Copy : DragDropEffects.None;
        e.Handled = true;
    }

    private void OnDrop(object sender, DragEventArgs e)
    {
        if (DataContext is not CreateInvoiceViewModel vm) return;
        if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return;
        if (e.Data.GetData(DataFormats.FileDrop) is not string[] files || files.Length == 0) return;
        vm.ApplyDroppedFile(files[0]);
    }

    /// <summary>
    /// Ensures mouse-wheel scrolling works over the whole page, not only inside multiline TextBoxes.
    /// Nested TextBoxes keep the wheel only while they still have content to scroll.
    /// </summary>
    private void OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (e.Handled) return;
        ScrollPage(e);
    }

    private void OnChildPreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (sender is not TextBox box) return;

        var viewer = GetScrollViewer(box);
        if (viewer is null)
        {
            ScrollPage(e);
            return;
        }

        var canScrollUp = e.Delta > 0 && viewer.VerticalOffset > 0;
        var canScrollDown = e.Delta < 0 && viewer.VerticalOffset < viewer.ScrollableHeight;
        if (canScrollUp || canScrollDown)
            return; // let the TextBox consume the wheel

        ScrollPage(e);
    }

    private void ScrollPage(MouseWheelEventArgs e)
    {
        if (PageScrollViewer.ScrollableHeight <= 0) return;
        PageScrollViewer.ScrollToVerticalOffset(PageScrollViewer.VerticalOffset - e.Delta);
        e.Handled = true;
    }

    private static ScrollViewer? GetScrollViewer(DependencyObject root)
    {
        if (root is ScrollViewer sv) return sv;
        for (var i = 0; i < System.Windows.Media.VisualTreeHelper.GetChildrenCount(root); i++)
        {
            var child = System.Windows.Media.VisualTreeHelper.GetChild(root, i);
            var result = GetScrollViewer(child);
            if (result is not null) return result;
        }
        return null;
    }
}
