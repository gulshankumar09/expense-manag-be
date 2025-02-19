namespace TranslationService.Configuration;

/// <summary>
/// Configuration settings for Azure Translator API
/// </summary>
public class AzureTranslateSettings
{
    /// <summary>
    /// The section name in the configuration file
    /// </summary>
    public const string SectionName = "AzureTranslate";

    /// <summary>
    /// The Azure Translator subscription key
    /// </summary>
    public string SubscriptionKey { get; set; } = string.Empty;

    /// <summary>
    /// The Azure Translator endpoint
    /// </summary>
    public string Endpoint { get; set; } = "https://api.cognitive.microsofttranslator.com/";

    /// <summary>
    /// The Azure region (e.g., "eastus")
    /// </summary>
    public string Region { get; set; } = string.Empty;

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

    /// <summary>
    /// Whether to use the premium tier for better translation quality
    /// </summary>
    public bool UsePremiumTier { get; set; } = false;

    /// <summary>
    /// Whether to include sentence boundaries in the response
    /// </summary>
    public bool IncludeSentenceLength { get; set; } = false;

    /// <summary>
    /// Whether to include alignment information in the response
    /// </summary>
    public bool IncludeAlignment { get; set; } = false;

    /// <summary>
    /// Whether to include source text in the response for verification
    /// </summary>
    public bool IncludeSourceText { get; set; } = false;

    /// <summary>
    /// Whether to filter profanity in translations
    /// </summary>
    public bool FilterProfanity { get; set; } = false;

    /// <summary>
    /// The category/domain ID for translations (e.g., "general")
    /// </summary>
    public string Category { get; set; } = "general";
}