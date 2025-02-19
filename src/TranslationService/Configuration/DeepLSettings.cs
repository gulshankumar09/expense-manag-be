namespace TranslationService.Configuration;

/// <summary>
/// Configuration settings for DeepL API
/// </summary>
public class DeepLSettings
{
    /// <summary>
    /// The section name in the configuration file
    /// </summary>
    public const string SectionName = "DeepL";

    /// <summary>
    /// The DeepL API key
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Whether to use the free API (api-free.deepl.com) or pro API (api.deepl.com)
    /// </summary>
    public bool UseFreeTier { get; set; } = false;

    /// <summary>
    /// Request timeout in seconds
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Maximum number of texts in a batch request
    /// </summary>
    public int MaxBatchSize { get; set; } = 50;

    /// <summary>
    /// Whether to enable request retries
    /// </summary>
    public bool EnableRetries { get; set; } = true;

    /// <summary>
    /// Maximum number of retry attempts
    /// </summary>
    public int MaxRetries { get; set; } = 3;

    /// <summary>
    /// Whether to split sentences (0 - no splitting, 1 - on punctuation, nonewlines - on punctuation except newlines)
    /// </summary>
    public string SplitSentences { get; set; } = "1";

    /// <summary>
    /// Whether to preserve formatting (0 - no, 1 - yes)
    /// </summary>
    public string PreserveFormatting { get; set; } = "0";

    /// <summary>
    /// Whether to use more formal language in translations
    /// </summary>
    public bool Formality { get; set; } = false;

    /// <summary>
    /// The tag handling mode (xml or html)
    /// </summary>
    public string? TagHandling { get; set; }

    /// <summary>
    /// List of XML tags to ignore
    /// </summary>
    public string[]? IgnoreTags { get; set; }

    /// <summary>
    /// Whether to use glossary for translations
    /// </summary>
    public bool UseGlossary { get; set; } = false;

    /// <summary>
    /// The glossary ID to use (if UseGlossary is true)
    /// </summary>
    public string? GlossaryId { get; set; }
}