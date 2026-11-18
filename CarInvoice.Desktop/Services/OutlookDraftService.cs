using CarInvoice.Core.Services;

namespace CarInvoice.Desktop.Services;

/// <summary>
/// Creates an Outlook draft email with the generated invoice attached (late-bound COM).
/// </summary>
public sealed class OutlookDraftService
{
    public bool TryCreateDraft(string to, string subject, string body, string? attachmentPath, out string? error)
    {
        error = null;
        var app = OutlookComHelper.CreateApplication();
        if (app is null)
        {
            error = "Classic Outlook is not available.";
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

            mail.GetType().InvokeMember("To", System.Reflection.BindingFlags.SetProperty, null, mail, new object[] { to });
            mail.GetType().InvokeMember("Subject", System.Reflection.BindingFlags.SetProperty, null, mail, new object[] { subject });
            mail.GetType().InvokeMember("Body", System.Reflection.BindingFlags.SetProperty, null, mail, new object[] { body });

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
}
