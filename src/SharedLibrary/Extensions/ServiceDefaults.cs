using SharedLibrary.Localization;

namespace SharedLibrary.Extensions;

/// <summary>
/// Provides access to the default globalization service
/// </summary>
public static class GlobalizationDefaults
{
    /// <summary>
    /// Gets or sets the default globalization service instance
    /// </summary>
    public static IGlobalizationService? DefaultService { get; set; }

    /// <summary>
    /// Gets the current globalization service instance
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the default service is not set</exception>
    public static IGlobalizationService Current => DefaultService ??
        throw new InvalidOperationException("Default globalization service not set");
}


/// <summary>
/// Provides access to the default localization service
/// </summary>
public static class LocalizationDefaults
{
    /// <summary>
    /// Gets or sets the default localization service instance
    /// </summary>
    public static ILocalizationService? DefaultService { get; set; }

    /// <summary>
    /// Gets the current localization service instance
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the default service is not set</exception>
    public static ILocalizationService Current => DefaultService ??
        throw new InvalidOperationException("Default localization service not set");
}