using TranslationService.Enums;

namespace TranslationService.Models;

/// <summary>
/// Represents a stored translation
/// </summary>
public class Translation
{
    public int Id { get; set; }
    public string SourceText { get; set; } = string.Empty;
    public string TranslatedText { get; set; } = string.Empty;
    public string SourceLanguage { get; set; } = string.Empty;
    public string TargetLanguage { get; set; } = string.Empty;
    public TranslationProvider Provider { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastUsedAt { get; set; }
    public int UseCount { get; set; }
} 