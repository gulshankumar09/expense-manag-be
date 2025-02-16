using SharedLibrary.Extensions;
using SharedLibrary.Localization;

namespace SharedLibrary.Utility;

/// <summary>
/// Extension methods for localization operations
/// </summary>
public static class LocalizationExtensionsUtility
{
    // String localization
    public static string Localize(this string key, ILocalizationService service)
        => service.GetLocalizedString(key);

    public static string Localize(this string key)
        => LocalizationDefaults.Current.GetLocalizedString(key);

    public static string Localize(this string key, string culture, ILocalizationService service)
        => service.GetLocalizedString(key, culture);

    public static string Localize(this string key, string culture)
        => LocalizationDefaults.Current.GetLocalizedString(key, culture);

    // String localization with parameters
    public static string Localize(this string key, object[] parameters, ILocalizationService service)
        => service.GetLocalizedString(key, parameters);

    public static string Localize(this string key, object[] parameters)
        => LocalizationDefaults.Current.GetLocalizedString(key, parameters);

    public static string Localize(this string key, string culture, object[] parameters, ILocalizationService service)
        => service.GetLocalizedString(key, culture, parameters);

    public static string Localize(this string key, string culture, object[] parameters)
        => LocalizationDefaults.Current.GetLocalizedString(key, culture, parameters);
}