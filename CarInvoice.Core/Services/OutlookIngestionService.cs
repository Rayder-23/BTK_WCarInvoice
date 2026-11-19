using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using CarInvoice.Core.DTOs;
using CarInvoice.Core.Enums;
using CarInvoice.Core.Interfaces;
using MsgReader.Outlook;

namespace CarInvoice.Core.Services;

/// <summary>
/// Ingests vehicle/customer data from live Classic Outlook (COM late-bind)
/// or offline .msg / .eml / clipboard text via MsgReader + regex extraction.
/// Never throws unhandled COM exceptions when New Outlook is active.
/// </summary>
public sealed class OutlookIngestionService : IOutlookIngestionService
{
    public const string UnavailableUserMessage =
        "Classic Outlook is not running (or New Outlook is active). Open Classic Outlook first, " +
        "then retry — or drop a .eml/.msg file / use Paste Email Text. CarInvoice never starts Outlook by itself.";

    private readonly IJapaneseTranslationService _translator;
    private readonly IChassisParserService _chassisParser;

    private static readonly (string Name, Regex Pattern)[] FieldPatterns =
    {
        ("Chassis", new Regex(@"(?:chassis|車台番号|frame\s*no\.?|chassis\s*no\.?)[:\s#]*([A-Z0-9\-\s]{6,30})", RegexOptions.IgnoreCase | RegexOptions.Compiled)),
        ("Make", new Regex(@"(?:make|maker|メーカー)[:\s]+([A-Za-z\u3040-\u30FF\u4E00-\u9FFF]+)", RegexOptions.IgnoreCase | RegexOptions.Compiled)),
        ("Model", new Regex(@"(?:model|車名|車種)[:\s]+([A-Za-z0-9\-\s\u3040-\u30FF\u4E00-\u9FFF]+)", RegexOptions.IgnoreCase | RegexOptions.Compiled)),
        ("Year", new Regex(@"(?:year|年式|model\s*year)[:\s]*((?:19|20)\d{2})", RegexOptions.IgnoreCase | RegexOptions.Compiled)),
        ("Mileage", new Regex(@"(?:mileage|走行距離|odometer|km)[:\s]*([\d,]+)\s*(?:km|miles|キロ)?", RegexOptions.IgnoreCase | RegexOptions.Compiled)),
        ("Color", new Regex(@"(?:color|colour|色|外装色)[:\s]+([A-Za-z\s\u3040-\u30FF\u4E00-\u9FFF]+)", RegexOptions.IgnoreCase | RegexOptions.Compiled)),
        ("Price", new Regex(@"(?:price|amount|売価|価格|FOB)[:\s]*[¥$€]?\s*([\d,]+(?:\.\d+)?)", RegexOptions.IgnoreCase | RegexOptions.Compiled)),
        ("Customer", new Regex(@"(?:customer|buyer|client|お客様|買主)[:\s]+([A-Za-z\s\.\u3040-\u30FF\u4E00-\u9FFF]+)", RegexOptions.IgnoreCase | RegexOptions.Compiled)),
        ("Grade", new Regex(@"(?:auction\s*grade|評価点|grade)[:\s]*([0-9]\.?[0-9]?|R|RA|S)", RegexOptions.IgnoreCase | RegexOptions.Compiled)),
        ("Fuel", new Regex(@"(?:fuel|燃料)[:\s]+([A-Za-z\u3040-\u30FF\u4E00-\u9FFF]+)", RegexOptions.IgnoreCase | RegexOptions.Compiled)),
        ("Transmission", new Regex(@"(?:transmission|シフト|ミッション)[:\s]+([A-Za-z0-9\u3040-\u30FF\u4E00-\u9FFF]+)", RegexOptions.IgnoreCase | RegexOptions.Compiled)),
    };

    private static readonly Regex JapaneseBlockRegex = new(@"[\u3040-\u30FF\u4E00-\u9FFF]{2,}", RegexOptions.Compiled);

    public OutlookIngestionService(IJapaneseTranslationService translator, IChassisParserService chassisParser)
    {
        _translator = translator;
        _chassisParser = chassisParser;
    }

    /// <inheritdoc />
    public bool IsOutlookAvailable()
        => CheckOutlookAvailability() == OutlookAvailabilityStatus.ClassicOutlookReady;

    /// <inheritdoc />
    public string GetUnavailableMessage() => UnavailableUserMessage;

    /// <inheritdoc />
    /// <remarks>
    /// Lightweight probe only: ProgID registered + OUTLOOK.EXE running.
    /// Never calls Activator.CreateInstance — that would launch Outlook on app start.
    /// </remarks>
    public OutlookAvailabilityStatus CheckOutlookAvailability()
    {
        try
        {
            if (!OutlookComHelper.IsOutlookInstalled())
                return OutlookAvailabilityStatus.NewOutlookOrUnavailable;

            if (!OutlookComHelper.IsOutlookProcessRunning())
                return OutlookAvailabilityStatus.NewOutlookOrUnavailable;

            return OutlookAvailabilityStatus.ClassicOutlookReady;
        }
        catch (COMException)
        {
            return OutlookAvailabilityStatus.NewOutlookOrUnavailable;
        }
        catch (InvalidOperationException)
        {
            return OutlookAvailabilityStatus.NewOutlookOrUnavailable;
        }
        catch
        {
            return OutlookAvailabilityStatus.NewOutlookOrUnavailable;
        }
    }

    /// <inheritdoc />
    public async Task<List<EmailIngestionResult>> FetchFromOutlookAsync(
        string? folderName = null,
        int maxCount = 20,
        bool unreadOnly = true,
        CancellationToken ct = default)
    {
        if (CheckOutlookAvailability() != OutlookAvailabilityStatus.ClassicOutlookReady)
            throw new InvalidOperationException(UnavailableUserMessage);

        var results = new List<EmailIngestionResult>();
        var app = OutlookComHelper.CreateApplication();
        if (app is null)
            throw new InvalidOperationException(UnavailableUserMessage);

        try
        {
            var session = OutlookComHelper.GetProperty(app, "Session")
                          ?? throw new InvalidOperationException("Unable to access Outlook MAPI session. Is Classic Outlook signed in?");
            object? folder;
            if (!string.IsNullOrWhiteSpace(folderName))
            {
                var inbox = OutlookComHelper.InvokeMethod(session, "GetDefaultFolder", 6); // olFolderInbox = 6
                var folders = OutlookComHelper.GetProperty(inbox!, "Folders");
                folder = OutlookComHelper.InvokeMethod(folders!, "Item", folderName)
                         ?? throw new InvalidOperationException($"Outlook folder '{folderName}' was not found under Inbox.");
            }
            else
            {
                folder = OutlookComHelper.InvokeMethod(session, "GetDefaultFolder", 6);
            }

            var items = OutlookComHelper.GetProperty(folder!, "Items");
            OutlookComHelper.InvokeMethod(items!, "Sort", "[ReceivedTime]", true);

            object? filtered = items;
            if (unreadOnly)
                filtered = OutlookComHelper.InvokeMethod(items!, "Restrict", "[UnRead] = true") ?? items;

            var countObj = OutlookComHelper.GetProperty(filtered!, "Count");
            var count = Convert.ToInt32(countObj ?? 0);
            var take = Math.Min(count, maxCount);

            for (var i = 1; i <= take; i++)
            {
                ct.ThrowIfCancellationRequested();
                var mail = OutlookComHelper.InvokeMethod(filtered!, "Item", i);
                if (mail is null) continue;

                var classObj = OutlookComHelper.GetProperty(mail, "Class");
                // olMail = 43
                if (classObj is not null && Convert.ToInt32(classObj) != 43)
                    continue;

                var subject = OutlookComHelper.GetProperty(mail, "Subject")?.ToString() ?? string.Empty;
                var senderName = OutlookComHelper.GetProperty(mail, "SenderName")?.ToString() ?? string.Empty;
                var senderEmail = OutlookComHelper.GetProperty(mail, "SenderEmailAddress")?.ToString() ?? string.Empty;
                var body = OutlookComHelper.GetProperty(mail, "Body")?.ToString() ?? string.Empty;
                var html = OutlookComHelper.GetProperty(mail, "HTMLBody")?.ToString() ?? string.Empty;
                DateTime? received = null;
                if (OutlookComHelper.GetProperty(mail, "ReceivedTime") is DateTime dt)
                    received = dt;

                var attachments = new List<string>();
                var atts = OutlookComHelper.GetProperty(mail, "Attachments");
                var attCount = Convert.ToInt32(OutlookComHelper.GetProperty(atts!, "Count") ?? 0);
                for (var a = 1; a <= attCount; a++)
                {
                    var att = OutlookComHelper.InvokeMethod(atts!, "Item", a);
                    var name = OutlookComHelper.GetProperty(att!, "FileName")?.ToString();
                    if (!string.IsNullOrWhiteSpace(name))
                        attachments.Add(name);
                }

                var result = new EmailIngestionResult
                {
                    Subject = subject,
                    Sender = string.IsNullOrWhiteSpace(senderName) ? senderEmail : senderName,
                    SenderEmail = senderEmail,
                    BodyText = body,
                    BodyHtml = html,
                    ReceivedAt = received,
                    AttachmentNames = attachments,
                    Extracted = ExtractFields($"{subject}\n{body}")
                };

                await EnrichExtractedAsync(result.Extracted, ct);
                results.Add(result);
            }
        }
        catch (COMException ex)
        {
            throw new InvalidOperationException(UnavailableUserMessage + $" ({ex.Message})", ex);
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (TargetInvocationException ex) when (ex.InnerException is COMException com)
        {
            throw new InvalidOperationException(UnavailableUserMessage + $" ({com.Message})", com);
        }
        finally
        {
            OutlookComHelper.ReleaseComObject(app);
        }

        return results;
    }

    /// <inheritdoc />
    public async Task<EmailIngestionResult> ParseMessageFileAsync(string filePath, CancellationToken ct = default)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException("Message file not found.", filePath);

        var ext = Path.GetExtension(filePath).ToLowerInvariant();
        EmailIngestionResult result;

        if (ext is ".msg")
        {
            using var msg = new Storage.Message(filePath);
            var body = msg.BodyText ?? StripHtml(msg.BodyHtml ?? string.Empty);
            result = new EmailIngestionResult
            {
                Subject = msg.Subject ?? string.Empty,
                Sender = msg.Sender?.DisplayName ?? msg.Sender?.Email ?? string.Empty,
                SenderEmail = msg.Sender?.Email ?? string.Empty,
                BodyText = body,
                BodyHtml = msg.BodyHtml ?? string.Empty,
                ReceivedAt = msg.SentOn?.DateTime ?? msg.ReceivedOn?.DateTime,
                AttachmentNames = msg.Attachments?
                    .OfType<Storage.Attachment>()
                    .Select(a => a.FileName ?? "attachment")
                    .ToList() ?? new List<string>(),
                Extracted = ExtractFields($"{msg.Subject}\n{body}")
            };
        }
        else if (ext is ".eml")
        {
            result = await ParseEmlFileAsync(filePath, ct);
        }
        else
        {
            throw new NotSupportedException($"Unsupported message file type: {ext}");
        }

        await EnrichExtractedAsync(result.Extracted, ct);
        return result;
    }

    /// <inheritdoc />
    public async Task<EmailIngestionResult> ParsePlainTextAsync(string text, CancellationToken ct = default)
    {
        var body = text?.Trim() ?? string.Empty;
        var subject = string.Empty;
        var subjectMatch = Regex.Match(body, @"^Subject:\s*(.+)$", RegexOptions.Multiline | RegexOptions.IgnoreCase);
        if (subjectMatch.Success)
            subject = subjectMatch.Groups[1].Value.Trim();

        var result = new EmailIngestionResult
        {
            Subject = subject,
            BodyText = body,
            Extracted = ExtractFields(body)
        };
        await EnrichExtractedAsync(result.Extracted, ct);
        return result;
    }

    /// <inheritdoc />
    public ExtractedVehicleFields ExtractFields(string text)
    {
        var fields = new ExtractedVehicleFields();
        if (string.IsNullOrWhiteSpace(text))
            return fields;

        foreach (var (name, pattern) in FieldPatterns)
        {
            var m = pattern.Match(text);
            if (!m.Success) continue;
            var value = m.Groups[1].Value.Trim().TrimEnd(',', '.', ';');
            switch (name)
            {
                case "Chassis":
                    fields.ChassisNumber = value;
                    break;
                case "Make": fields.Make = value; break;
                case "Model": fields.Model = value; break;
                case "Year" when int.TryParse(value, out var y): fields.Year = y; break;
                case "Mileage":
                    var miles = value.Replace(",", "");
                    if (int.TryParse(miles, out var km)) fields.Mileage = km;
                    break;
                case "Color": fields.Color = value; break;
                case "Price":
                    var price = value.Replace(",", "");
                    if (decimal.TryParse(price, out var p)) fields.Price = p;
                    break;
                case "Customer": fields.CustomerName = value; break;
                case "Grade": fields.AuctionGrade = value.ToUpperInvariant(); break;
                case "Fuel": fields.FuelType = value; break;
                case "Transmission": fields.Transmission = value; break;
            }
        }

        var jpBlocks = JapaneseBlockRegex.Matches(text)
            .Select(m => m.Value)
            .Distinct()
            .ToList();
        if (jpBlocks.Count > 0)
            fields.JapaneseRemarks = string.Join(" / ", jpBlocks);

        return fields;
    }

    private async Task<EmailIngestionResult> ParseEmlFileAsync(string filePath, CancellationToken ct)
    {
        // Prefer MsgReader MIME reader when available; fall back to header/body split.
        try
        {
            var eml = MsgReader.Mime.Message.Load(new FileInfo(filePath));
            if (eml is not null)
            {
                var subject = eml.Headers?.Subject ?? string.Empty;
                var fromDisplay = eml.Headers?.From?.DisplayName ?? string.Empty;
                var fromEmail = eml.Headers?.From?.Address ?? string.Empty;
                var html = eml.HtmlBody?.GetBodyAsText() ?? string.Empty;
                var plain = eml.TextBody?.GetBodyAsText()
                            ?? (!string.IsNullOrWhiteSpace(html) ? StripHtml(html) : string.Empty);
                if (string.IsNullOrWhiteSpace(plain))
                    plain = StripHtml(html);

                DateTime? received = eml.Headers is null ? null : eml.Headers.DateSent.DateTime;

                var attachments = new List<string>();
                if (eml.Attachments is not null)
                {
                    foreach (var att in eml.Attachments)
                    {
                        var name = att.FileName;
                        if (!string.IsNullOrWhiteSpace(name))
                            attachments.Add(name);
                    }
                }

                return new EmailIngestionResult
                {
                    Subject = subject,
                    Sender = string.IsNullOrWhiteSpace(fromDisplay) ? fromEmail : fromDisplay,
                    SenderEmail = fromEmail,
                    BodyText = plain,
                    BodyHtml = html,
                    ReceivedAt = received,
                    AttachmentNames = attachments,
                    Extracted = ExtractFields($"{subject}\n{plain}")
                };
            }
        }
        catch
        {
            // fall through to raw text parse
        }

        ct.ThrowIfCancellationRequested();
        var raw = await File.ReadAllTextAsync(filePath, ct);
        var subjectHeader = Regex.Match(raw, @"^Subject:\s*(.+)$", RegexOptions.Multiline | RegexOptions.IgnoreCase).Groups[1].Value.Trim();
        var from = Regex.Match(raw, @"^From:\s*(.+)$", RegexOptions.Multiline | RegexOptions.IgnoreCase).Groups[1].Value.Trim();
        var bodyStart = raw.IndexOf("\r\n\r\n", StringComparison.Ordinal);
        if (bodyStart < 0) bodyStart = raw.IndexOf("\n\n", StringComparison.Ordinal);
        var body = bodyStart >= 0 ? raw[(bodyStart + (raw[bodyStart] == '\r' ? 4 : 2))..].Trim() : raw;
        body = StripHtml(body);

        return new EmailIngestionResult
        {
            Subject = subjectHeader,
            Sender = from,
            SenderEmail = ExtractEmailAddress(from),
            BodyText = body,
            Extracted = ExtractFields($"{subjectHeader}\n{body}")
        };
    }

    private async Task EnrichExtractedAsync(ExtractedVehicleFields fields, CancellationToken ct)
    {
        var chassisRaw = fields.ChassisNumber ?? string.Empty;
        if (!string.IsNullOrWhiteSpace(chassisRaw))
        {
            var parsed = await _chassisParser.ParseAsync(chassisRaw, ct);
            if (!string.IsNullOrWhiteSpace(parsed.ChassisCode))
                fields.ChassisCode = parsed.ChassisCode;
            if (!string.IsNullOrWhiteSpace(parsed.ChassisNumber))
                fields.ChassisNumber = parsed.ChassisNumber;
            fields.Make ??= parsed.SuggestedMake;
            fields.Model ??= parsed.SuggestedModel;
            fields.FuelType ??= parsed.SuggestedFuelType;
        }

        if (!string.IsNullOrWhiteSpace(fields.JapaneseRemarks))
            fields.TranslatedRemarks = await _translator.TranslateAsync(fields.JapaneseRemarks, true, ct);

        if (!string.IsNullOrWhiteSpace(fields.Color) && JapaneseBlockRegex.IsMatch(fields.Color))
            fields.Color = await _translator.TranslateAsync(fields.Color, false, ct);

        if (!string.IsNullOrWhiteSpace(fields.FuelType) && JapaneseBlockRegex.IsMatch(fields.FuelType))
            fields.FuelType = await _translator.TranslateAsync(fields.FuelType, false, ct);

        if (!string.IsNullOrWhiteSpace(fields.Transmission) && JapaneseBlockRegex.IsMatch(fields.Transmission))
            fields.Transmission = await _translator.TranslateAsync(fields.Transmission, false, ct);
    }

    private static string ExtractEmailAddress(string fromHeader)
    {
        var m = Regex.Match(fromHeader, @"<([^>]+)>");
        if (m.Success) return m.Groups[1].Value.Trim();
        m = Regex.Match(fromHeader, @"[\w\.\-+]+@[\w\.\-]+\.\w+");
        return m.Success ? m.Value : string.Empty;
    }

    private static string StripHtml(string html)
    {
        if (string.IsNullOrWhiteSpace(html)) return string.Empty;
        var text = Regex.Replace(html, "<[^>]+>", " ");
        text = System.Net.WebUtility.HtmlDecode(text);
        return Regex.Replace(text, @"\s+", " ").Trim();
    }
}
