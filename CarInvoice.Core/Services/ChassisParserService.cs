using System.Text.RegularExpressions;
using CarInvoice.Core.DTOs;
using CarInvoice.Core.Interfaces;

namespace CarInvoice.Core.Services;

/// <summary>
/// Parses messy combined chassis strings (e.g. DBA-NZE161-7048291) into code + number,
/// suggests make/model from the local lookup table, and warns on duplicate chassis numbers.
/// </summary>
public sealed class ChassisParserService : IChassisParserService
{
    private static readonly Regex VinRegex = new(@"\b([A-HJ-NPR-Z0-9]{17})\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex ChassisCodeRegex = new(
        @"(?:DBA|CBA|LDA|DAA|5AA|ABA|GF|GH|LA|DBA)?[-\s]?(?<code>[A-Z]{1,3}[0-9]{1,3}[A-Z]?)\s*[-#]?\s*(?<num>[0-9]{4,8})",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex LooseCodeRegex = new(@"\b(?<code>[A-Z]{1,3}\d{1,3}[A-Z]?)\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex LooseNumberRegex = new(@"\b(?<num>\d{5,8})\b", RegexOptions.Compiled);

    private readonly IUnitOfWork _uow;

    public ChassisParserService(IUnitOfWork uow)
    {
        _uow = uow;
    }

    /// <inheritdoc />
    public string Normalize(string chassisCode, string chassisNumber)
    {
        var combined = $"{chassisCode}{chassisNumber}";
        return new string(combined.Where(c => !char.IsWhiteSpace(c) && c != '-' && c != '#').ToArray()).ToUpperInvariant();
    }

    /// <inheritdoc />
    public async Task<ChassisParseResult> ParseAsync(string rawInput, CancellationToken ct = default)
    {
        var result = new ChassisParseResult();
        if (string.IsNullOrWhiteSpace(rawInput))
            return result;

        var input = rawInput.Trim();

        var vinMatch = VinRegex.Match(input);
        if (vinMatch.Success)
        {
            result.IsVin = true;
            result.ChassisNumber = vinMatch.Groups[1].Value.ToUpperInvariant();
            result.ChassisCode = result.ChassisNumber.Length >= 8 ? result.ChassisNumber[..8] : result.ChassisNumber;
            result.FullChassisNormalized = Normalize(result.ChassisCode, result.ChassisNumber);
            await ApplyLookupAndDuplicateAsync(result, ct);
            return result;
        }

        var match = ChassisCodeRegex.Match(input.Replace("_", "-"));
        if (match.Success)
        {
            result.ChassisCode = match.Groups["code"].Value.ToUpperInvariant();
            result.ChassisNumber = match.Groups["num"].Value;
        }
        else
        {
            // Fallback: split on whitespace / hyphen
            var parts = Regex.Split(input, @"[\s\-#]+")
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .Select(p => p.Trim())
                .ToList();

            // Strip Japanese registration prefixes like DBA-
            parts = parts
                .Where(p => !Regex.IsMatch(p, @"^(DBA|CBA|LDA|DAA|5AA|ABA|GF|GH)$", RegexOptions.IgnoreCase))
                .ToList();

            var codePart = parts.FirstOrDefault(p => LooseCodeRegex.IsMatch(p) && !p.All(char.IsDigit));
            var numPart = parts.FirstOrDefault(p => LooseNumberRegex.IsMatch(p) || (p.All(char.IsDigit) && p.Length >= 4));

            if (codePart is not null)
                result.ChassisCode = LooseCodeRegex.Match(codePart).Groups["code"].Value.ToUpperInvariant();
            if (numPart is not null)
                result.ChassisNumber = LooseNumberRegex.IsMatch(numPart)
                    ? LooseNumberRegex.Match(numPart).Groups["num"].Value
                    : numPart;
        }

        result.FullChassisNormalized = Normalize(result.ChassisCode, result.ChassisNumber);
        await ApplyLookupAndDuplicateAsync(result, ct);
        return result;
    }

    private async Task ApplyLookupAndDuplicateAsync(ChassisParseResult result, CancellationToken ct)
    {
        if (!string.IsNullOrWhiteSpace(result.ChassisCode))
        {
            var lookup = await _uow.ChassisLookups.FindByCodeAsync(result.ChassisCode, ct);
            if (lookup is not null)
            {
                result.SuggestedMake = lookup.DefaultMake;
                result.SuggestedModel = lookup.DefaultModel;
                result.SuggestedEngineCc = lookup.DefaultEngineCc;
                result.SuggestedFuelType = lookup.DefaultFuelType;
            }
        }

        if (!string.IsNullOrWhiteSpace(result.ChassisNumber))
        {
            var existing = await _uow.Vehicles.FindByChassisNumberAsync(result.ChassisNumber, ct)
                           ?? await _uow.Vehicles.FindByNormalizedAsync(result.FullChassisNormalized, ct);
            if (existing is not null)
            {
                result.IsDuplicate = true;
                result.ExistingVehicle = existing;
                result.DuplicateWarning =
                    $"Chassis {existing.ChassisCode}-{existing.ChassisNumber} already exists " +
                    $"({existing.Make} {existing.Model}, {existing.Mileage} {existing.MileageUnit}" +
                    (string.IsNullOrWhiteSpace(existing.HistoryNotes) ? ")" : $", history: {existing.HistoryNotes})");
            }
        }
    }
}
