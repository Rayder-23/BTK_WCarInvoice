using System.Text.RegularExpressions;
using CarInvoice.Core.DTOs;
using CarInvoice.Core.Entities;
using CarInvoice.Core.Interfaces;

namespace CarInvoice.Core.Services;

/// <summary>
/// Parses messy combined chassis strings, resolves multi-model lookups,
/// and warns on duplicate chassis numbers.
/// </summary>
public sealed class ChassisParserService : IChassisParserService
{
    private static readonly Regex VinRegex = new(@"\b([A-HJ-NPR-Z0-9]{17})\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex ChassisCodeRegex = new(
        @"(?:DBA|CBA|LDA|DAA|5AA|ABA|GF|GH|LA)?[-\s]?(?<code>[A-Z]{1,4}[0-9]{1,3}[A-Z0-9]{0,3})\s*[-#]?\s*(?<num>[0-9]{4,8})",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex LooseCodeRegex = new(@"\b(?<code>[A-Z]{1,4}\d{1,3}[A-Z0-9]{0,3})\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex LooseNumberRegex = new(@"\b(?<num>\d{5,8})\b", RegexOptions.Compiled);
    private static readonly Regex NumericRangeRegex = new(@"^(?<lo>\d+)\s*[-–]\s*(?<hi>\d+)$", RegexOptions.Compiled);

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
            var parts = Regex.Split(input, @"[\s\-#]+")
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .Select(p => p.Trim())
                .ToList();

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

    /// <summary>Resolves all lookup rows for a chassis code, ranking serial-range hits first.</summary>
    public async Task<ChassisLookupResult> ResolveLookupsAsync(
        string chassisCode,
        string? chassisNumber = null,
        CancellationToken ct = default)
    {
        var lookup = new ChassisLookupResult();
        if (string.IsNullOrWhiteSpace(chassisCode))
            return lookup;

        var matches = await _uow.ChassisLookups.FindAllByCodeAsync(chassisCode, ct);
        if (matches.Count == 0)
            return lookup;

        ChassisCodeLookup? recommended = null;
        if (!string.IsNullOrWhiteSpace(chassisNumber))
        {
            recommended = matches.FirstOrDefault(m => SerialMatches(m.SerialPrefixOrRange, chassisNumber));
            if (recommended is not null)
            {
                matches = matches
                    .OrderByDescending(m => ReferenceEquals(m, recommended))
                    .ThenBy(m => m.DefaultMake)
                    .ThenBy(m => m.DefaultModel)
                    .ToList();
            }
        }

        lookup.Matches = matches;
        lookup.RecommendedMatch = recommended;
        return lookup;
    }

    private async Task ApplyLookupAndDuplicateAsync(ChassisParseResult result, CancellationToken ct)
    {
        if (!string.IsNullOrWhiteSpace(result.ChassisCode))
        {
            result.Lookup = await ResolveLookupsAsync(result.ChassisCode, result.ChassisNumber, ct);

            if (result.Lookup.SingleMatch is { } single)
                ApplySuggestion(result, single);
            else if (result.Lookup.RecommendedMatch is { } recommended && result.Lookup.Matches.Count > 1)
            {
                // Recommend but do not auto-fill when multiple models exist.
                result.SuggestedMake = null;
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

    private static void ApplySuggestion(ChassisParseResult result, ChassisCodeLookup lookup)
    {
        result.SuggestedMake = lookup.DefaultMake;
        result.SuggestedModel = lookup.DefaultModel;
        result.SuggestedEngineCc = lookup.DefaultEngineCc;
        result.SuggestedFuelType = lookup.DefaultFuelType;
        result.SuggestedBodyType = lookup.DefaultBodyType;
        result.SuggestedDoors = lookup.DefaultDoors;
        result.SuggestedSeats = lookup.DefaultSeats;
        result.SuggestedEngineNumber = lookup.DefaultEngineNumber;
    }

    /// <summary>
    /// Matches serial against "2000000-2999999" numeric ranges or "70xxxxx" / "70*" prefix patterns.
    /// </summary>
    public static bool SerialMatches(string? pattern, string chassisNumber)
    {
        if (string.IsNullOrWhiteSpace(pattern) || string.IsNullOrWhiteSpace(chassisNumber))
            return false;

        var serial = chassisNumber.Trim();
        var p = pattern.Trim();

        var range = NumericRangeRegex.Match(p);
        if (range.Success && long.TryParse(serial, out var serialNum)
            && long.TryParse(range.Groups["lo"].Value, out var lo)
            && long.TryParse(range.Groups["hi"].Value, out var hi))
        {
            return serialNum >= lo && serialNum <= hi;
        }

        // Prefix with x/X/?/* wildcards for remaining digits, e.g. 70xxxxx
        var prefixChars = new string(p.TakeWhile(c => char.IsLetterOrDigit(c) && c is not ('x' or 'X' or '?' or '*')).ToArray());
        if (prefixChars.Length > 0 && serial.StartsWith(prefixChars, StringComparison.OrdinalIgnoreCase))
        {
            var wildcardLen = p.Length - prefixChars.Length;
            if (wildcardLen <= 0) return serial.Equals(prefixChars, StringComparison.OrdinalIgnoreCase);
            return serial.Length >= prefixChars.Length
                   && (wildcardLen == 0 || serial.Length - prefixChars.Length <= Math.Max(wildcardLen, 8));
        }

        return serial.StartsWith(p, StringComparison.OrdinalIgnoreCase);
    }
}
