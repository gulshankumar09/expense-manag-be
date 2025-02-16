using System.Globalization;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using SharedLibrary.Configuration;

namespace SharedLibrary.Localization;

/// <summary>
/// Service for handling localization and translations
/// </summary>
public class LocalizationService : ILocalizationService
{
    private readonly IStringLocalizer _localizer;
    private readonly LocalizationSettings _settings;

    public LocalizationService(
        IStringLocalizerFactory localizerFactory,
        IOptions<LocalizationSettings> settings)
    {
        _settings = settings.Value;
        _localizer = localizerFactory.Create("SharedResources", "SharedLibrary");
    }

    /// <inheritdoc/>
    public string GetLocalizedString(string key)
    {
        return _localizer[key];
    }

    /// <inheritdoc/>
    public string GetLocalizedString(string key, params object[] arguments)
    {
        return _localizer[key, arguments];
    }

    /// <inheritdoc/>
    public string GetLocalizedString(string key, string culture)
    {
        var currentCulture = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = new CultureInfo(culture);
            CultureInfo.CurrentUICulture = new CultureInfo(culture);
            return _localizer[key];
        }
        finally
        {
            CultureInfo.CurrentCulture = currentCulture;
            CultureInfo.CurrentUICulture = currentCulture;
        }
    }

    /// <inheritdoc/>
    public IDictionary<string, string> GetAllStrings(string culture)
    {
        var currentCulture = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = new CultureInfo(culture);
            CultureInfo.CurrentUICulture = new CultureInfo(culture);
            return _localizer.GetAllStrings()
                .ToDictionary(ls => ls.Name, ls => ls.Value);
        }
        finally
        {
            CultureInfo.CurrentCulture = currentCulture;
            CultureInfo.CurrentUICulture = currentCulture;
        }
    }

    /// <inheritdoc/>
    public CultureInfo GetCurrentCulture()
    {
        return CultureInfo.CurrentCulture;
    }

    /// <inheritdoc/>
    public string[] GetSupportedCultures()
    {
        return _settings.SupportedCultures;
    }

    /// <inheritdoc/>
    public string GetString(string key, params object[] args)
    {
        var localizedString = _localizer[key, args];
        if (localizedString.ResourceNotFound && _settings.ThrowOnMissingTranslation)
        {
            throw new KeyNotFoundException($"Resource key not found: {key}");
        }
        return localizedString.Value;
    }

    /// <inheritdoc/>
    public string GetString(string key, string culture, params object[] args)
    {
        var currentCulture = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = new CultureInfo(culture);
            CultureInfo.CurrentUICulture = new CultureInfo(culture);
            return GetString(key, args);
        }
        finally
        {
            CultureInfo.CurrentCulture = currentCulture;
            CultureInfo.CurrentUICulture = currentCulture;
        }
    }
}