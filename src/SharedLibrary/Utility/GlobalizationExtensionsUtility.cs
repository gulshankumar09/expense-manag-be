using SharedLibrary.Extensions;
using SharedLibrary.Localization;

namespace SharedLibrary.Utility;

/// <summary>
/// Extension methods for globalization operations
/// </summary>
public static class GlobalizationExtensionsUtility
{
    // Currency formatting
    public static string ToCurrency(this decimal value, IGlobalizationService service)
        => service.FormatCurrency(value);

    public static string ToCurrency(this decimal value)
        => GlobalizationDefaults.Current.FormatCurrency(value);

    public static string ToCurrency(this decimal value, string culture, IGlobalizationService service)
        => service.FormatCurrency(value, culture);

    public static string ToCurrency(this decimal value, string culture)
        => GlobalizationDefaults.Current.FormatCurrency(value, culture);

    // Number formatting
    public static string ToNumber(this decimal value, IGlobalizationService service)
        => service.FormatNumber(value);

    public static string ToNumber(this decimal value)
        => GlobalizationDefaults.Current.FormatNumber(value);

    public static string ToNumber(this decimal value, string culture, IGlobalizationService service)
        => service.FormatNumber(value, culture);

    public static string ToNumber(this decimal value, string culture)
        => GlobalizationDefaults.Current.FormatNumber(value, culture);

    // Date formatting
    public static string ToFormattedDate(this DateTime value, IGlobalizationService service)
        => service.FormatDate(value);

    public static string ToFormattedDate(this DateTime value)
        => GlobalizationDefaults.Current.FormatDate(value);

    public static string ToFormattedDate(this DateTime value, string culture, IGlobalizationService service)
        => service.FormatDate(value, culture);

    public static string ToFormattedDate(this DateTime value, string culture)
        => GlobalizationDefaults.Current.FormatDate(value, culture);

    // Time formatting
    public static string ToFormattedTime(this DateTime value, IGlobalizationService service)
        => service.FormatTime(value);

    public static string ToFormattedTime(this DateTime value)
        => GlobalizationDefaults.Current.FormatTime(value);

    public static string ToFormattedTime(this DateTime value, string culture, IGlobalizationService service)
        => service.FormatTime(value, culture);

    public static string ToFormattedTime(this DateTime value, string culture)
        => GlobalizationDefaults.Current.FormatTime(value, culture);

    // DateTime formatting
    public static string ToFormattedDateTime(this DateTime value, IGlobalizationService service)
        => service.FormatDateTime(value);

    public static string ToFormattedDateTime(this DateTime value)
        => GlobalizationDefaults.Current.FormatDateTime(value);

    public static string ToFormattedDateTime(this DateTime value, string culture, IGlobalizationService service)
        => service.FormatDateTime(value, culture);

    public static string ToFormattedDateTime(this DateTime value, string culture)
        => GlobalizationDefaults.Current.FormatDateTime(value, culture);
}