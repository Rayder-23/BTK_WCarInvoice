using System.Diagnostics;
using System.Text;
using CarInvoice.Core.Enums;
using CarInvoice.Core.Interfaces;
using CarInvoice.Core.Services;

namespace CarInvoice.Desktop.Services;

/// <summary>
/// Creates an Outlook draft with the generated invoice attached (Classic COM),
/// or falls back to explorer /select + mailto: when New Outlook / COM is unavailable.
/// </summary>
public sealed class OutlookDraftService
{
    private readonly IOutlookIngestionService _outlook;

    public OutlookDraftService(IOutlookIngestionService outlook)
    {
        _outlook = outlook;
    }

    public OutlookDraftOutcome CreateDraftOrFallback(
        string to,
        string subject,
        string body,
        string? attachmentPath)
    {
        string? comError = null;
        if (_outlook.CheckOutlookAvailability() == OutlookAvailabilityStatus.ClassicOutlookReady
            && TryCreateClassicDraft(to, subject, body, attachmentPath, out comError))
        {
            return new OutlookDraftOutcome(
                Success: true,
                UsedMailtoFallback: false,
                Message: "Classic Outlook draft opened with invoice attached.");
        }

        try
        {
            if (!string.IsNullOrWhiteSpace(attachmentPath) && File.Exists(attachmentPath))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "explorer.exe",
                    Arguments = $"/select,\"{attachmentPath}\"",
                    UseShellExecute = true
                });
            }

            var summary = TruncateForMailto(body);
            var mailtoBody = string.IsNullOrWhiteSpace(attachmentPath)
                ? summary
                : $"{summary}\n\nPlease attach: {Path.GetFileName(attachmentPath)}\n(The file is selected in the folder that just opened.)";

            var mailto = BuildMailtoUri(to, subject, mailtoBody);
            Process.Start(new ProcessStartInfo
            {
                FileName = mailto,
                UseShellExecute = true
            });

            var hint = string.IsNullOrWhiteSpace(comError)
                ? _outlook.GetUnavailableMessage()
                : comError;

            return new OutlookDraftOutcome(
                Success: true,
                UsedMailtoFallback: true,
                Message: "Opened default mail client and selected the .docx. Attach it manually — Classic Outlook COM is unavailable. " + hint);
        }
        catch (Exception ex)
        {
            return new OutlookDraftOutcome(
                Success: false,
                UsedMailtoFallback: true,
                Message: $"Could not open mail fallback: {ex.Message}");
        }
    }

    private static bool TryCreateClassicDraft(
        string to,
        string subject,
        string body,
        string? attachmentPath,
        out string? error)
    {
        error = null;
        var app = OutlookComHelper.CreateApplication();
        if (app is null)
        {
            error = "Classic Outlook COM could not be started.";
            return false;
        }

        try
        {
            // olMailItem = 0
            var mail = OutlookComHelper.InvokeMethod(app, "CreateItem", 0);
            if (mail is null)
            {
                error = "Could not create Outlook mail item.";
                return false;
            }

            OutlookComHelper.SetProperty(mail, "To", to ?? string.Empty);
            OutlookComHelper.SetProperty(mail, "Subject", subject ?? string.Empty);
            OutlookComHelper.SetProperty(mail, "Body", body ?? string.Empty);

            if (!string.IsNullOrWhiteSpace(attachmentPath) && File.Exists(attachmentPath))
            {
                var attachments = OutlookComHelper.GetProperty(mail, "Attachments");
                OutlookComHelper.InvokeMethod(attachments!, "Add", attachmentPath);
            }

            OutlookComHelper.InvokeMethod(mail, "Display", true);
            return true;
        }
        catch (Exception ex)
        {
            error = ex.Message;
            return false;
        }
        finally
        {
            OutlookComHelper.ReleaseComObject(app);
        }
    }

    private static string BuildMailtoUri(string to, string subject, string body)
    {
        var sb = new StringBuilder("mailto:");
        if (!string.IsNullOrWhiteSpace(to))
            sb.Append(Uri.EscapeDataString(to.Trim()).Replace("%40", "@"));
        sb.Append('?');
        sb.Append("subject=").Append(Uri.EscapeDataString(subject ?? string.Empty));
        sb.Append("&body=").Append(Uri.EscapeDataString(body ?? string.Empty));
        return sb.ToString();
    }

    private static string TruncateForMailto(string text, int maxLen = 1200)
    {
        if (string.IsNullOrEmpty(text)) return string.Empty;
        text = text.Replace("\r\n", "\n").Trim();
        return text.Length <= maxLen ? text : text[..maxLen] + "…";
    }
}

public sealed record OutlookDraftOutcome(bool Success, bool UsedMailtoFallback, string Message);
