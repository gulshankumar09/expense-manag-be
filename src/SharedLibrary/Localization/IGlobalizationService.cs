namespace SharedLibrary.Localization;

/// <summary>
/// Interface for globalization service
/// </summary>
public interface IGlobalizationService
{
    /// <summary>
    /// Formats a decimal value as currency
    /// </summary>
    /// <param name="amount">The amount to format</param>
    /// <param name="culture">Optional culture code (e.g., "en-US")</param>
    /// <returns>Formatted currency string</returns>
    string FormatCurrency(decimal amount, string? culture = null);

    /// <summary>
    /// Formats a decimal value as a number
    /// </summary>
    /// <param name="number">The number to format</param>
    /// <param name="culture">Optional culture code (e.g., "en-US")</param>
    /// <returns>Formatted number string</returns>
    string FormatNumber(decimal number, string? culture = null);

    /// <summary>
    /// Formats a date value
    /// </summary>
    /// <param name="date">The date to format</param>
    /// <param name="culture">Optional culture code (e.g., "en-US")</param>
    /// <returns>Formatted date string</returns>
    string FormatDate(DateTime date, string? culture = null);

    /// <summary>
    /// Formats a time value
    /// </summary>
    /// <param name="time">The time to format</param>
    /// <param name="culture">Optional culture code (e.g., "en-US")</param>
    /// <returns>Formatted time string</returns>
    string FormatTime(DateTime time, string? culture = null);

    /// <summary>
    /// Formats a date and time value
    /// </summary>
    /// <param name="dateTime">The date and time to format</param>
    /// <param name="culture">Optional culture code (e.g., "en-US")</param>
    /// <returns>Formatted date and time string</returns>
    string FormatDateTime(DateTime dateTime, string? culture = null);

    /// <summary>
    /// Parses a currency string to decimal
    /// </summary>
    /// <param name="value">The currency string to parse</param>
    /// <param name="culture">Optional culture code (e.g., "en-US")</param>
    /// <returns>Parsed decimal value</returns>
    decimal ParseCurrency(string value, string? culture = null);

    /// <summary>
    /// Parses a number string to decimal
    /// </summary>
    /// <param name="value">The number string to parse</param>
    /// <param name="culture">Optional culture code (e.g., "en-US")</param>
    /// <returns>Parsed decimal value</returns>
    decimal ParseNumber(string value, string? culture = null);

    /// <summary>
    /// Parses a date string to DateTime
    /// </summary>
    /// <param name="value">The date string to parse</param>
    /// <param name="culture">Optional culture code (e.g., "en-US")</param>
    /// <returns>Parsed DateTime value</returns>
    DateTime ParseDate(string value, string? culture = null);

    /// <summary>
    /// Gets the currency symbol for a culture
    /// </summary>
    /// <param name="culture">Optional culture code (e.g., "en-US")</param>
    /// <returns>Currency symbol</returns>
    string GetCurrencySymbol(string? culture = null);
}