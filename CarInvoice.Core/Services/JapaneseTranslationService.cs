using System.Text;
using System.Text.Json;
using CarInvoice.Core.Entities;
using CarInvoice.Core.Interfaces;

namespace CarInvoice.Core.Services;

/// <summary>
/// Offline rule-based Japanese → English translator backed by SQLite glossary
/// plus optional JSON override file for user-added terms.
/// </summary>
public sealed class JapaneseTranslationService : IJapaneseTranslationService
{
    private readonly IUnitOfWork _uow;
    private readonly string _jsonOverridePath;
    private List<(string Japanese, string English)> _terms = new();
    private readonly object _lock = new();

    public JapaneseTranslationService(IUnitOfWork uow, string? jsonOverridePath = null)
    {
        _uow = uow;
        _jsonOverridePath = jsonOverridePath
            ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "CarInvoice", "glossary-overrides.json");
    }

    /// <inheritdoc />
    public async Task ReloadAsync(CancellationToken ct = default)
    {
        var dbTerms = await _uow.Glossary.GetAllOrderedAsync(ct);
        var combined = dbTerms
            .Select(t => (t.JapaneseTerm, t.EnglishTranslation))
            .ToList();

        if (File.Exists(_jsonOverridePath))
        {
            try
            {
                var json = await File.ReadAllTextAsync(_jsonOverridePath, ct);
                var overrides = JsonSerializer.Deserialize<List<GlossaryOverride>>(json) ?? new();
                foreach (var o in overrides.Where(x => !string.IsNullOrWhiteSpace(x.Japanese)))
                {
                    combined.RemoveAll(t => string.Equals(t.JapaneseTerm, o.Japanese, StringComparison.Ordinal));
                    combined.Add((o.Japanese, o.English));
                }
            }
            catch
            {
                // Ignore corrupt override file; DB terms still apply.
            }
        }

        // Longest-first so multi-char terms win over shorter substrings.
        lock (_lock)
        {
            _terms = combined
                .OrderByDescending(t => t.JapaneseTerm.Length)
                .Select(t => (t.JapaneseTerm, t.EnglishTranslation))
                .ToList();
        }
    }

    /// <inheritdoc />
    public async Task<string> TranslateAsync(string text, bool keepOriginalInParentheses = true, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(text))
            return text;

        if (_terms.Count == 0)
            await ReloadAsync(ct);

        List<(string Japanese, string English)> snapshot;
        lock (_lock)
            snapshot = _terms.ToList();

        var sb = new StringBuilder(text);
        var working = text;

        foreach (var (japanese, english) in snapshot)
        {
            if (string.IsNullOrEmpty(japanese) || !working.Contains(japanese, StringComparison.Ordinal))
                continue;

            var replacement = keepOriginalInParentheses
                ? $"{english} ({japanese})"
                : english;

            working = working.Replace(japanese, replacement, StringComparison.Ordinal);
        }

        return working;
    }

    /// <inheritdoc />
    public async Task AddOrUpdateTermAsync(string japanese, string english, string category, CancellationToken ct = default)
    {
        var existing = await _uow.Glossary.FindByJapaneseTermAsync(japanese, ct);
        if (existing is null)
        {
            await _uow.Glossary.AddAsync(new JapaneseTermGlossary
            {
                JapaneseTerm = japanese,
                EnglishTranslation = english,
                Category = category
            }, ct);
        }
        else
        {
            existing.EnglishTranslation = english;
            existing.Category = category;
            _uow.Glossary.Update(existing);
        }

        await _uow.SaveChangesAsync(ct);

        // Also persist to JSON overrides for easy backup/edit.
        Directory.CreateDirectory(Path.GetDirectoryName(_jsonOverridePath)!);
        var list = File.Exists(_jsonOverridePath)
            ? JsonSerializer.Deserialize<List<GlossaryOverride>>(await File.ReadAllTextAsync(_jsonOverridePath, ct)) ?? new()
            : new List<GlossaryOverride>();

        var item = list.FirstOrDefault(x => string.Equals(x.Japanese, japanese, StringComparison.Ordinal));
        if (item is null)
            list.Add(new GlossaryOverride { Japanese = japanese, English = english, Category = category });
        else
        {
            item.English = english;
            item.Category = category;
        }

        await File.WriteAllTextAsync(_jsonOverridePath,
            JsonSerializer.Serialize(list, new JsonSerializerOptions { WriteIndented = true }), ct);

        await ReloadAsync(ct);
    }

    private sealed class GlossaryOverride
    {
        public string Japanese { get; set; } = string.Empty;
        public string English { get; set; } = string.Empty;
        public string Category { get; set; } = "Grade/Spec";
    }
}
