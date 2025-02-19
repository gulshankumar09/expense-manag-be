namespace TranslationService.Configuration;

/// <summary>
/// Configuration settings for Google Cloud Translation API
/// </summary>
public class GoogleTranslateSettings
{
    /// <summary>
    /// The section name in the configuration file
    /// </summary>
    public const string SectionName = "GoogleTranslate";

    /// <summary>
    /// The Google Cloud project ID
    /// </summary>
    public string ProjectId { get; set; } = string.Empty;

    /// <summary>
    /// The path to the Google Cloud credentials JSON file
    /// </summary>
    public string CredentialsPath { get; set; } = string.Empty;

    /// <summary>
    /// The Google Cloud credentials JSON content (alternative to CredentialsPath)
    /// </summary>
    public string? CredentialsJson { get; set; }

    /// <summary>
    /// The location of the service (e.g., "global")
    /// </summary>
    public string Location { get; set; } = "global";

    /// <summary>
    /// Whether to use the premium model for better translation quality
    /// </summary>
    public bool UsePremiumModel { get; set; } = false;

    /// <summary>
    /// Request timeout in seconds
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Maximum number of texts in a batch request
    /// </summary>
    public int MaxBatchSize { get; set; } = 100;

    /// <summary>
    /// Whether to enable request retries
    /// </summary>
    public bool EnableRetries { get; set; } = true;

    /// <summary>
    /// Maximum number of retry attempts
    /// </summary>
    public int MaxRetries { get; set; } = 3;
}