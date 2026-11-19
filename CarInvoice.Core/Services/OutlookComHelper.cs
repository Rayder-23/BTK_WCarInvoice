using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;

namespace CarInvoice.Core.Services;

/// <summary>
/// Late-bound Classic Outlook COM wrapper using ProgID so the app builds
/// without Office PIAs and runs gracefully when Outlook is missing or New Outlook is active.
/// Never auto-launches Outlook — connect only when OUTLOOK.EXE is already running.
/// </summary>
public static class OutlookComHelper
{
    public const string ProgId = "Outlook.Application";

    /// <summary>True when Outlook.Application ProgID is registered (Classic Outlook COM).</summary>
    public static bool IsOutlookInstalled()
    {
        try
        {
            return Type.GetTypeFromProgID(ProgId) is not null;
        }
        catch (COMException)
        {
            return false;
        }
        catch (InvalidOperationException)
        {
            return false;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>True when Classic Outlook (OUTLOOK.EXE) is already running.</summary>
    public static bool IsOutlookProcessRunning()
    {
        try
        {
            return Process.GetProcessesByName("OUTLOOK").Length > 0;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Attaches to a running Classic Outlook instance. Returns null when Outlook is closed,
    /// New Outlook-only, or COM fails — never starts Outlook.
    /// </summary>
    public static object? TryConnectRunningApplication()
    {
        // Gate on process first so Activator.CreateInstance cannot cold-start Outlook.
        if (!IsOutlookProcessRunning())
            return null;

        try
        {
            // Outlook is a singleton: while OUTLOOK.EXE is running, CreateInstance
            // returns the existing instance and does not spawn a second process.
            var type = Type.GetTypeFromProgID(ProgId);
            if (type is null)
                return null;
            return Activator.CreateInstance(type);
        }
        catch (COMException)
        {
            return null;
        }
        catch (InvalidOperationException)
        {
            return null;
        }
        catch (Exception)
        {
            return null;
        }
    }

    /// <summary>Alias kept for call sites — never launches Outlook if it is closed.</summary>
    public static object? CreateApplication() => TryConnectRunningApplication();

    public static object? GetProperty(object target, string name)
        => target.GetType().InvokeMember(name, BindingFlags.GetProperty, null, target, null);

    public static void SetProperty(object target, string name, object? value)
        => target.GetType().InvokeMember(name, BindingFlags.SetProperty, null, target, new[] { value });

    public static object? InvokeMethod(object target, string name, params object?[] args)
        => target.GetType().InvokeMember(name, BindingFlags.InvokeMethod, null, target, args);

    public static void ReleaseComObject(object? comObject)
    {
        if (comObject is null) return;
        try
        {
            if (Marshal.IsComObject(comObject))
                Marshal.FinalReleaseComObject(comObject);
        }
        catch
        {
            // ignored
        }
    }
}
