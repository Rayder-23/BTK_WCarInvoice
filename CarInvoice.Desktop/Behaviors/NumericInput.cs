using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CarInvoice.Desktop.Behaviors;

public enum NumericInputKind
{
    /// <summary>Digits only (year, odometer, seats, doors, CC, weight).</summary>
    Integer,

    /// <summary>Non-negative decimal with optional '.' (price, fees).</summary>
    Decimal
}

/// <summary>
/// Attached behavior: blocks letters and other non-numeric input (typing + paste).
/// </summary>
public static class NumericInput
{
    public static readonly DependencyProperty KindProperty =
        DependencyProperty.RegisterAttached(
            "Kind",
            typeof(NumericInputKind?),
            typeof(NumericInput),
            new PropertyMetadata(null, OnKindChanged));

    public static void SetKind(DependencyObject element, NumericInputKind? value)
        => element.SetValue(KindProperty, value);

    public static NumericInputKind? GetKind(DependencyObject element)
        => (NumericInputKind?)element.GetValue(KindProperty);

    private static void OnKindChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not TextBox box)
            return;

        box.PreviewTextInput -= OnPreviewTextInput;
        box.PreviewKeyDown -= OnPreviewKeyDown;
        DataObject.RemovePastingHandler(box, OnPaste);

        if (e.NewValue is null)
            return;

        box.PreviewTextInput += OnPreviewTextInput;
        box.PreviewKeyDown += OnPreviewKeyDown;
        DataObject.AddPastingHandler(box, OnPaste);
        InputMethod.SetIsInputMethodEnabled(box, false);
    }

    private static void OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        // Space is not raised as PreviewTextInput
        if (e.Key == Key.Space)
            e.Handled = true;
    }

    private static void OnPreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        if (sender is not TextBox box)
            return;

        var kind = GetKind(box);
        if (kind is null)
            return;

        e.Handled = !IsValidProposed(box, e.Text, kind.Value);
    }

    private static void OnPaste(object sender, DataObjectPastingEventArgs e)
    {
        if (sender is not TextBox box)
            return;

        var kind = GetKind(box);
        if (kind is null)
            return;

        if (!e.SourceDataObject.GetDataPresent(DataFormats.UnicodeText, true))
        {
            e.CancelCommand();
            return;
        }

        var text = e.SourceDataObject.GetData(DataFormats.UnicodeText) as string ?? string.Empty;
        text = SanitizePaste(text, kind.Value);
        if (!IsValidProposed(box, text, kind.Value))
        {
            e.CancelCommand();
            return;
        }

        // Replace paste payload with sanitized digits/decimal only
        e.DataObject = new DataObject(DataFormats.UnicodeText, text);
    }

    private static string SanitizePaste(string text, NumericInputKind kind)
    {
        text = text.Trim().Replace(",", string.Empty);
        if (kind == NumericInputKind.Integer)
            return Regex.Replace(text, @"[^\d]", string.Empty);

        // Keep digits and a single decimal point
        var cleaned = Regex.Replace(text, @"[^\d.]", string.Empty);
        var firstDot = cleaned.IndexOf('.');
        if (firstDot < 0)
            return cleaned;
        return cleaned[..(firstDot + 1)] + cleaned[(firstDot + 1)..].Replace(".", string.Empty);
    }

    private static bool IsValidProposed(TextBox box, string incoming, NumericInputKind kind)
    {
        if (string.IsNullOrEmpty(incoming))
            return false;

        var current = box.Text ?? string.Empty;
        var start = box.SelectionStart;
        var length = box.SelectionLength;
        if (start < 0 || start > current.Length)
            start = current.Length;
        if (length < 0 || start + length > current.Length)
            length = 0;

        var proposed = current.Remove(start, length).Insert(start, incoming);
        return IsValidValue(proposed, kind);
    }

    private static bool IsValidValue(string value, NumericInputKind kind)
    {
        if (string.IsNullOrEmpty(value))
            return true;

        return kind switch
        {
            NumericInputKind.Integer => value.All(char.IsDigit),
            NumericInputKind.Decimal => IsValidDecimal(value),
            _ => true
        };
    }

    private static bool IsValidDecimal(string value)
    {
        // Allow intermediate states while typing: "12." or "."
        if (value == ".")
            return true;

        if (value.Count(c => c == '.') > 1)
            return false;

        if (!value.All(c => char.IsDigit(c) || c == '.'))
            return false;

        return decimal.TryParse(
                   value.TrimEnd('.'),
                   NumberStyles.AllowDecimalPoint,
                   CultureInfo.InvariantCulture,
                   out _)
               || value.EndsWith(".", StringComparison.Ordinal);
    }
}
