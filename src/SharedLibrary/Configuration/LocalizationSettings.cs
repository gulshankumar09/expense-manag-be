namespace SharedLibrary.Configuration;

/// <summary>
/// Configuration settings for localization and globalization
/// </summary>
public class LocalizationSettings
{
    /// <summary>
    /// The section name in the configuration file
    /// </summary>
    public const string SectionName = "Localization";

    /// <summary>
    /// Default culture for the application
    /// </summary>
    public string DefaultCulture { get; set; } = "en-US";

    /// <summary>
    /// Supported cultures for the application
    /// </summary>
    public string[] SupportedCultures { get; set; } = ["en-US"];

    /// <summary>
    /// Resource path for localization files
    /// </summary>
    public string ResourcesPath { get; set; } = "Resources";

    /// <summary>
    /// Whether to use the browser's preferred culture
    /// </summary>
    public bool UseUserPreferredCulture { get; set; } = true;

    /// <summary>
    /// Whether to fallback to parent culture if translation is not found
    /// </summary>
    public bool FallBackToParentCulture { get; set; } = true;

    /// <summary>
    /// Whether to throw exception when a resource is not found
    /// </summary>
    public bool ThrowOnMissingTranslation { get; set; } = false;

    /// <summary>
    /// Default currency format for the application
    /// </summary>
    public string DefaultCurrencyFormat { get; set; } = "C2";

    /// <summary>
    /// Default date format for the application
    /// </summary>
    public string DefaultDateFormat { get; set; } = "d";

    /// <summary>
    /// Default time format for the application
    /// </summary>
    public string DefaultTimeFormat { get; set; } = "t";

    /// <summary>
    /// Default number format for the application
    /// </summary>
    public string DefaultNumberFormat { get; set; } = "N2";

    /// <summary>
    /// Whether to use invariant culture for specific operations
    /// </summary>
    public bool UseInvariantCulture { get; set; } = false;
}