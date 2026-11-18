using System.Reflection;
using System.Runtime.InteropServices;

namespace CarInvoice.Core.Services;

/// <summary>
/// Late-bound Classic Outlook COM wrapper using ProgID so the app builds
/// without Office PIAs and runs gracefully when Outlook is missing.
/// </summary>
public static class OutlookComHelper
{
    public const string ProgId = "Outlook.Application";

    /// <summary>Returns true when Outlook.Application ProgID is registered.</summary>
    public static bool IsOutlookInstalled()
    {
        try
        {
            var type = Type.GetTypeFromProgID(ProgId);
            return type is not null;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>Creates or attaches to a running Outlook.Application instance.</summary>
    public static object? CreateApplication()
    {
        try
        {
            var type = Type.GetTypeFromProgID(ProgId);
            if (type is null)
                return null;
            return Activator.CreateInstance(type);
        }
        catch (COMException)
        {
            return null;
        }
        catch (Exception)
        {
            return null;
        }
    }

    public static object? GetProperty(object target, string name)
        => target.GetType().InvokeMember(name, BindingFlags.GetProperty, null, target, null);

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
