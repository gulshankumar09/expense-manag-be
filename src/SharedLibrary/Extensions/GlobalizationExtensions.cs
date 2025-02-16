using System.Globalization;
using SharedLibrary.Localization;

namespace SharedLibrary.Extensions;

/// <summary>
/// Extension methods for globalization operations
/// </summary>
public static class GlobalizationExtensions
{
    /// <summary>
    /// Formats a decimal value as currency using the specified culture
    /// </summary>
    public static string ToCurrency(this decimal value, IGlobalizationService globalizationService, string? culture = null)
        => globalizationService.FormatCurrency(value, culture);

    /// <summary>
    /// Formats a decimal value as a number using the specified culture
    /// </summary>
    public static string ToFormattedNumber(this decimal value, IGlobalizationService globalizationService, string? culture = null)
        => globalizationService.FormatNumber(value, culture);

    /// <summary>
    /// Formats a DateTime value as a date string using the specified culture
    /// </summary>
    public static string ToFormattedDate(this DateTime value, IGlobalizationService globalizationService, string? culture = null)
        => globalizationService.FormatDate(value, culture);

    /// <summary>
    /// Formats a DateTime value as a time string using the specified culture
    /// </summary>
    public static string ToFormattedTime(this DateTime value, IGlobalizationService globalizationService, string? culture = null)
        => globalizationService.FormatTime(value, culture);

    /// <summary>
    /// Formats a DateTime value as a date and time string using the specified culture
    /// </summary>
    public static string ToFormattedDateTime(this DateTime value, IGlobalizationService globalizationService, string? culture = null)
        => globalizationService.FormatDateTime(value, culture);

    /// <summary>
    /// Parses a string as a currency value using the specified culture
    /// </summary>
    public static decimal ToCurrencyValue(this string value, IGlobalizationService globalizationService, string? culture = null)
        => globalizationService.ParseCurrency(value, culture);

    /// <summary>
    /// Parses a string as a number value using the specified culture
    /// </summary>
    public static decimal ToNumberValue(this string value, IGlobalizationService globalizationService, string? culture = null)
        => globalizationService.ParseNumber(value, culture);

    /// <summary>
    /// Parses a string as a date value using the specified culture
    /// </summary>
    public static DateTime ToDateValue(this string value, IGlobalizationService globalizationService, string? culture = null)
        => globalizationService.ParseDate(value, culture);
}