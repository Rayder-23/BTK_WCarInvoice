namespace CarInvoice.Core.Entities;

/// <summary>
/// Offline Japanese → English automotive glossary term.
/// </summary>
public class JapaneseTermGlossary
{
    public int Id { get; set; }
    public string JapaneseTerm { get; set; } = string.Empty;
    public string EnglishTranslation { get; set; } = string.Empty;

    /// <summary>Category: Color, Condition/History, Grade/Spec, Fuel/Transmission.</summary>
    public string Category { get; set; } = "Grade/Spec";
}
