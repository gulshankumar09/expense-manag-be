using System.Globalization;

namespace SharedLibrary.Localization;

/// <summary>
/// Interface for localization service
/// </summary>
public interface ILocalizationService
{
    /// <summary>
    /// Gets a localized string by key
    /// </summary>
    /// <param name="key">The resource key</param>
    /// <returns>The localized string</returns>
    string GetLocalizedString(string key);

    /// <summary>
    /// Gets a localized string by key with format arguments
    /// </summary>
    /// <param name="key">The resource key</param>
    /// <param name="arguments">Format arguments</param>
    /// <returns>The formatted localized string</returns>
    string GetLocalizedString(string key, params object[] arguments);

    /// <summary>
    /// Gets a localized string by key for a specific culture
    /// </summary>
    /// <param name="key">The resource key</param>
    /// <param name="culture">The culture code (e.g., "en-US")</param>
    /// <returns>The localized string</returns>
    string GetLocalizedString(string key, string culture);

    /// <summary>
    /// Gets all localized strings for a specific culture
    /// </summary>
    /// <param name="culture">The culture code (e.g., "en-US")</param>
    /// <returns>Dictionary of resource keys and their localized values</returns>
    IDictionary<string, string> GetAllStrings(string culture);

    /// <summary>
    /// Gets the current culture
    /// </summary>
    /// <returns>The current CultureInfo</returns>
    CultureInfo GetCurrentCulture();

    /// <summary>
    /// Gets the list of supported cultures
    /// </summary>
    /// <returns>Array of supported culture codes</returns>
    string[] GetSupportedCultures();
}