namespace TranslationService.Configuration;

/// <summary>
/// Configuration settings for LibreTranslate API
/// </summary>
public class LibreTranslateSettings
{
    /// <summary>
    /// The section name in the configuration file
    /// </summary>
    public const string SectionName = "LibreTranslate";

    /// <summary>
    /// The LibreTranslate API endpoint URL
    /// </summary>
    public string ApiUrl { get; set; } = "https://libretranslate.com";

    /// <summary>
    /// The API key (optional, required for some instances)
    /// </summary>
    public string? ApiKey { get; set; }

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
    /// Whether to use HTTPS for API requests
    /// </summary>
    public bool UseHttps { get; set; } = true;

    /// <summary>
    /// Whether to verify SSL certificates
    /// </summary>
    public bool VerifySsl { get; set; } = true;

    /// <summary>
    /// Whether to prefer faster translation over quality
    /// </summary>
    public bool PreferFast { get; set; } = false;

    /// <summary>
    /// Whether to include source text in the response for verification
    /// </summary>
    public bool IncludeSourceText { get; set; } = false;

    /// <summary>
    /// Whether to include confidence scores in the response
    /// </summary>
    public bool IncludeConfidence { get; set; } = false;
}