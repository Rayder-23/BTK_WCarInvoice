using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace CarInvoice.Desktop.Behaviors;

/// <summary>
/// App-wide ComboBox mouse-wheel fix (WPF best practice):
/// <list type="bullet">
/// <item>Closed: ignore wheel on the combo (avoids accidental selection changes)
///       and forward the wheel to the nearest parent ScrollViewer.</item>
/// <item>Open: do not mark Handled — the popup list ScrollViewer must receive the wheel.</item>
/// </list>
/// Register once via <see cref="Register"/> from App startup.
/// </summary>
public static class ComboBoxWheelFix
{
    private static bool _registered;

    public static void Register()
    {
        if (_registered) return;
        _registered = true;

        EventManager.RegisterClassHandler(
            typeof(ComboBox),
            UIElement.PreviewMouseWheelEvent,
            new MouseWheelEventHandler(OnPreviewMouseWheel),
            handledEventsToo: false);

        // Stop parent ScrollViewers from jumping when the mouse enters a partially-visible item
        EventManager.RegisterClassHandler(
            typeof(ComboBoxItem),
            FrameworkElement.RequestBringIntoViewEvent,
            new RequestBringIntoViewEventHandler(OnComboBoxItemRequestBringIntoView));
    }

    private static void OnComboBoxItemRequestBringIntoView(object sender, RequestBringIntoViewEventArgs e)
    {
        // Allow keyboard navigation to scroll items into view
        if (Keyboard.IsKeyDown(Key.Up) || Keyboard.IsKeyDown(Key.Down)
            || Keyboard.IsKeyDown(Key.PageUp) || Keyboard.IsKeyDown(Key.PageDown)
            || Keyboard.IsKeyDown(Key.Home) || Keyboard.IsKeyDown(Key.End))
            return;

        e.Handled = true;
    }

    private static void OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (e.Handled || sender is not ComboBox combo)
            return;

        // Dropdown open → leave the event alone so the popup ScrollViewer scrolls.
        if (combo.IsDropDownOpen)
            return;

        // Closed → don't change SelectedIndex with the wheel; scroll the page instead.
        e.Handled = true;
        ScrollParent(combo, e.Delta);
    }

    /// <summary>True when any ComboBox in the visual tree (or a popup over it) has an open dropdown.</summary>
    public static bool IsAnyDropDownOpen(DependencyObject? root)
    {
        if (root is null) return false;

        if (root is ComboBox { IsDropDownOpen: true })
            return true;

        // Popup items live outside the page tree — detect via mouse-over
        if (Mouse.DirectlyOver is DependencyObject over
            && (FindAncestor<ComboBoxItem>(over) is not null
                || FindAncestor<ComboBox>(over) is { IsDropDownOpen: true }))
            return true;

        var count = VisualTreeHelper.GetChildrenCount(root);
        for (var i = 0; i < count; i++)
        {
            if (IsAnyDropDownOpen(VisualTreeHelper.GetChild(root, i)))
                return true;
        }

        return false;
    }

    private static void ScrollParent(DependencyObject from, int delta)
    {
        var viewer = FindAncestor<ScrollViewer>(from);
        if (viewer is null || viewer.ScrollableHeight <= 0)
            return;

        viewer.ScrollToVerticalOffset(viewer.VerticalOffset - delta);
    }

    private static T? FindAncestor<T>(DependencyObject? current) where T : DependencyObject
    {
        while (current is not null)
        {
            if (current is T match)
                return match;
            current = current is Visual or System.Windows.Media.Media3D.Visual3D
                ? VisualTreeHelper.GetParent(current)
                : LogicalTreeHelper.GetParent(current);
        }

        return null;
    }
}
