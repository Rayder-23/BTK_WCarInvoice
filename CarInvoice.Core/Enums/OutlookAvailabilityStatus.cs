namespace CarInvoice.Core.Enums;

/// <summary>
/// Result of probing Classic Outlook (Win32 COM) vs New Outlook / unavailable.
/// </summary>
public enum OutlookAvailabilityStatus
{
    /// <summary>Outlook.Application ProgID responds and a MAPI session can be opened.</summary>
    ClassicOutlookReady,

    /// <summary>
    /// New Outlook (WebView2) is active, Classic Outlook is closed/missing,
    /// or COM creation failed — use .msg/.eml drop or clipboard paste instead.
    /// </summary>
    NewOutlookOrUnavailable
}
