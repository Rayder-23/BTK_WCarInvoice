using System.Text.RegularExpressions;
using CarInvoice.Core.Entities;
using CarInvoice.Core.Interfaces;

namespace CarInvoice.Core.Services;

/// <summary>
/// Self-learning for chassis lookups and Japanese glossary terms from completed invoices.
/// </summary>
public sealed class DictionaryLearningService : IDictionaryLearningService
{
    private static readonly Regex JapaneseRegex = new(@"[\u3040-\u30FF\u4E00-\u9FAF]", RegexOptions.Compiled);
    private static readonly Regex JapaneseTokenRegex = new(@"[\u3040-\u30FF\u4E00-\u9FAF]{2,}", RegexOptions.Compiled);

    private readonly IUnitOfWork _uow;
    private readonly IJapaneseTranslationService _translator;

    public DictionaryLearningService(IUnitOfWork uow, IJapaneseTranslationService translator)
    {
        _uow = uow;
        _translator = translator;
    }

    /// <inheritdoc />
    public async Task<DictionaryLearningResult> LearnFromInvoiceAsync(
        string chassisCode,
        string make,
        string model,
        int? engineCc,
        string? bodyType,
        int? doors,
        int? seats,
        string? fuelType,
        string? engineNumber,
        string? chassisNumber,
        string? exteriorColor,
        string? historyNotes,
        string? rawSourceText,
        CancellationToken ct = default)
    {
        var result = new DictionaryLearningResult();

        if (!string.IsNullOrWhiteSpace(chassisCode)
            && !string.IsNullOrWhiteSpace(make)
            && !string.IsNullOrWhiteSpace(model))
        {
            var code = chassisCode.Trim().ToUpperInvariant();
            var existing = await _uow.ChassisLookups.FindExactAsync(code, make.Trim(), model.Trim(), null, ct);
            if (existing is null)
            {
                // Also treat case-insensitive make/model as existing.
                var all = await _uow.ChassisLookups.FindAllByCodeAsync(code, ct);
                existing = all.FirstOrDefault(c =>
                    c.DefaultMake.Equals(make.Trim(), StringComparison.OrdinalIgnoreCase) &&
                    c.DefaultModel.Equals(model.Trim(), StringComparison.OrdinalIgnoreCase));
            }

            if (existing is null)
            {
                try
                {
                    // Do not invent a serial range on learn — keeps the composite unique key stable
                    // (ChassisCode + Make + Model + null) and avoids duplicate-key collisions.
                    var entity = new ChassisCodeLookup
                    {
                        ChassisCode = code,
                        DefaultMake = make.Trim().ToUpperInvariant(),
                        DefaultModel = model.Trim().ToUpperInvariant(),
                        DefaultEngineCc = engineCc,
                        DefaultBodyType = string.IsNullOrWhiteSpace(bodyType) ? null : bodyType.Trim().ToUpperInvariant(),
                        DefaultDoors = doors,
                        DefaultSeats = seats,
                        DefaultFuelType = string.IsNullOrWhiteSpace(fuelType) ? "PETROL" : fuelType.Trim().ToUpperInvariant(),
                        DefaultEngineNumber = string.IsNullOrWhiteSpace(engineNumber) ? null : engineNumber.Trim().ToUpperInvariant(),
                        Notes = "Auto-learned from invoice"
                    };
                    await _uow.ChassisLookups.AddAsync(entity, ct);
                    await _uow.SaveChangesAsync(ct);
                    result.ChassisLearned = true;
                    result.ChassisMessage = $"Learned new chassis mapping: {entity.ChassisCode} → {entity.DefaultMake} {entity.DefaultModel}";
                }
                catch
                {
                    // Unique race / already learned elsewhere — ignore; invoice save must not fail.
                }
            }
        }

        try
        {
            var glossaryAdded = 0;
            glossaryAdded += await TryLearnColorPairAsync(rawSourceText, exteriorColor, ct);
            glossaryAdded += await TryLearnHistoryPairsAsync(rawSourceText, historyNotes, ct);

            if (glossaryAdded > 0)
            {
                result.GlossaryTermsAdded = glossaryAdded;
                result.GlossaryMessage = glossaryAdded == 1
                    ? "Learned 1 glossary term"
                    : $"Learned {glossaryAdded} glossary terms";
                await _translator.ReloadAsync(ct);
            }
        }
        catch
        {
            // Glossary learn is best-effort.
        }

        return result;
    }

    private async Task<int> TryLearnColorPairAsync(string? rawSource, string? englishColor, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(englishColor) || JapaneseRegex.IsMatch(englishColor))
            return 0;
        if (string.IsNullOrWhiteSpace(rawSource))
            return 0;

        var jpTokens = JapaneseTokenRegex.Matches(rawSource)
            .Select(m => m.Value)
            .Distinct()
            .ToList();
        if (jpTokens.Count == 0) return 0;

        // Prefer a JP token that appears near a color label, else first JP token when english color was typed.
        var colorNear = Regex.Match(
            rawSource,
            @"(?:color|colour|色|外装色)[:\s]*([\u3040-\u30FF\u4E00-\u9FAF]{2,})",
            RegexOptions.IgnoreCase);
        var jp = colorNear.Success ? colorNear.Groups[1].Value : jpTokens[0];

        var existing = await _uow.Glossary.FindByJapaneseTermAsync(jp, ct);
        if (existing is not null) return 0;

        await _translator.AddOrUpdateTermAsync(jp, englishColor.Trim(), "Color", ct);
        return 1;
    }

    private async Task<int> TryLearnHistoryPairsAsync(string? rawSource, string? historyNotes, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(historyNotes) || string.IsNullOrWhiteSpace(rawSource))
            return 0;

        // Pattern: 日本語 (English) produced by translator, or user-typed English alongside JP in source.
        var parenPairs = Regex.Matches(historyNotes, @"([\u3040-\u30FF\u4E00-\u9FAF]{2,})\s*\(([^)]+)\)");
        var added = 0;
        foreach (Match m in parenPairs)
        {
            var jp = m.Groups[1].Value.Trim();
            var en = m.Groups[2].Value.Trim();
            if (string.IsNullOrWhiteSpace(en) || JapaneseRegex.IsMatch(en)) continue;
            var existing = await _uow.Glossary.FindByJapaneseTermAsync(jp, ct);
            if (existing is not null) continue;
            await _translator.AddOrUpdateTermAsync(jp, en, "Condition/History", ct);
            added++;
        }

        return added;
    }

    private static string? InferSerialPrefix(string? chassisNumber)
    {
        if (string.IsNullOrWhiteSpace(chassisNumber) || chassisNumber.Length < 4)
            return null;
        // Keep first 2 digits + x wildcards for remaining length (e.g. 70xxxxx).
        var digits = new string(chassisNumber.Where(char.IsDigit).ToArray());
        if (digits.Length < 4) return null;
        var prefixLen = Math.Min(2, digits.Length);
        return digits[..prefixLen] + new string('x', Math.Min(digits.Length - prefixLen, 6));
    }
}
