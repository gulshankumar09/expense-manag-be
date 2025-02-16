using System.Globalization;
using Microsoft.Extensions.Options;
using SharedLibrary.Configuration;

namespace SharedLibrary.Localization;

/// <summary>
/// Service for handling globalization (formatting of dates, numbers, and currencies)
/// </summary>
public class GlobalizationService : IGlobalizationService
{
    private readonly LocalizationSettings _settings;

    public GlobalizationService(IOptions<LocalizationSettings> settings)
    {
        _settings = settings.Value;
    }

    /// <inheritdoc/>
    public string FormatCurrency(decimal amount, string? culture = null)
    {
        var cultureInfo = GetCultureInfo(culture);
        return amount.ToString(_settings.DefaultCurrencyFormat, cultureInfo);
    }

    /// <inheritdoc/>
    public string FormatNumber(decimal number, string? culture = null)
    {
        var cultureInfo = GetCultureInfo(culture);
        return number.ToString(_settings.DefaultNumberFormat, cultureInfo);
    }

    /// <inheritdoc/>
    public string FormatDate(DateTime date, string? culture = null)
    {
        var cultureInfo = GetCultureInfo(culture);
        return date.ToString(_settings.DefaultDateFormat, cultureInfo);
    }

    /// <inheritdoc/>
    public string FormatTime(DateTime time, string? culture = null)
    {
        var cultureInfo = GetCultureInfo(culture);
        return time.ToString(_settings.DefaultTimeFormat, cultureInfo);
    }

    /// <inheritdoc/>
    public string FormatDateTime(DateTime dateTime, string? culture = null)
    {
        var cultureInfo = GetCultureInfo(culture);
        return dateTime.ToString($"{_settings.DefaultDateFormat} {_settings.DefaultTimeFormat}", cultureInfo);
    }

    /// <inheritdoc/>
    public decimal ParseCurrency(string value, string? culture = null)
    {
        var cultureInfo = GetCultureInfo(culture);
        if (decimal.TryParse(value, NumberStyles.Currency, cultureInfo, out decimal result))
        {
            return result;
        }
        throw new FormatException($"Unable to parse currency value: {value}");
    }

    /// <inheritdoc/>
    public decimal ParseNumber(string value, string? culture = null)
    {
        var cultureInfo = GetCultureInfo(culture);
        if (decimal.TryParse(value, NumberStyles.Number, cultureInfo, out decimal result))
        {
            return result;
        }
        throw new FormatException($"Unable to parse number value: {value}");
    }

    /// <inheritdoc/>
    public DateTime ParseDate(string value, string? culture = null)
    {
        var cultureInfo = GetCultureInfo(culture);
        if (DateTime.TryParse(value, cultureInfo, DateTimeStyles.None, out DateTime result))
        {
            return result;
        }
        throw new FormatException($"Unable to parse date value: {value}");
    }

    /// <inheritdoc/>
    public string GetCurrencySymbol(string? culture = null)
    {
        var cultureInfo = GetCultureInfo(culture);
        return cultureInfo.NumberFormat.CurrencySymbol;
    }

    private CultureInfo GetCultureInfo(string? culture)
    {
        if (_settings.UseInvariantCulture)
        {
            return CultureInfo.InvariantCulture;
        }

        if (!string.IsNullOrEmpty(culture))
        {
            return new CultureInfo(culture);
        }

        return CultureInfo.CurrentCulture;
    }
}