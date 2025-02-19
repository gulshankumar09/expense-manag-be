using TranslationService.Enums;

namespace TranslationService.Configuration;

/// <summary>
/// Configuration settings for the translation service
/// </summary>
public class TranslationServiceSettings
{
    /// <summary>
    /// The section name in the configuration file
    /// </summary>
    public const string SectionName = "TranslationService";

    /// <summary>
    /// The default translation provider to use
    /// </summary>
    public TranslationProvider DefaultProvider { get; set; } = TranslationProvider.Google;

    /// <summary>
    /// Whether to enable fallback to other providers if the primary provider fails
    /// </summary>
    public bool EnableFallback { get; set; } = true;

    /// <summary>
    /// The order of providers to try when fallback is enabled
    /// </summary>
    public TranslationProvider[] FallbackOrder { get; set; } =
    {
        TranslationProvider.Google,
        TranslationProvider.Azure,
        TranslationProvider.DeepL,
        TranslationProvider.LibreTranslate
    };

    /// <summary>
    /// Whether to cache translations across providers
    /// </summary>
    public bool ShareCache { get; set; } = true;

    /// <summary>
    /// Cache duration in hours
    /// </summary>
    public int CacheDurationHours { get; set; } = 24;

    /// <summary>
    /// Maximum cache size in megabytes
    /// </summary>
    public int MaxCacheSizeMB { get; set; } = 1024;
}