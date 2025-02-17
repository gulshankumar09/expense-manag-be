using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using SharedLibrary.Configuration;
using SharedLibrary.Localization.Entities;

namespace SharedLibrary.Localization.Services;

/// <summary>
/// Database-driven implementation of the localization service
/// </summary>
public class DatabaseTranslationService : ILocalizationService
{
    private readonly DbContext _dbContext;
    private readonly IMemoryCache _cache;
    private readonly ILogger<DatabaseTranslationService> _logger;
    private readonly LocalizationSettings _settings;
    private readonly MemoryCacheEntryOptions _cacheOptions;

    public DatabaseTranslationService(
        DbContext dbContext,
        IMemoryCache cache,
        ILogger<DatabaseTranslationService> logger,
        LocalizationSettings settings)
    {
        _dbContext = dbContext;
        _cache = cache;
        _logger = logger;
        _settings = settings;

        _cacheOptions = new MemoryCacheEntryOptions()
            .SetSlidingExpiration(TimeSpan.FromHours(1))
            .SetAbsoluteExpiration(TimeSpan.FromHours(24));
    }

    public string GetLocalizedString(string key)
        => GetLocalizedString(key, CultureInfo.CurrentCulture.Name);

    public string GetLocalizedString(string key, params object[] arguments)
        => string.Format(GetLocalizedString(key), arguments);

    public string GetLocalizedString(string key, string culture)
    {
        var cacheKey = $"translation_{culture}_{key}";

        return _cache.GetOrCreate(cacheKey, entry =>
        {
            entry.SetOptions(_cacheOptions);

            var translation = _dbContext.Set<Translation>()
                .FirstOrDefault(t => t.Key == key && t.Culture == culture);

            if (translation != null)
                return translation.Value;

            // Try parent culture if enabled
            if (_settings.FallBackToParentCulture)
            {
                var parentCulture = CultureInfo.GetCultureInfo(culture).Parent.Name;
                if (!string.IsNullOrEmpty(parentCulture))
                {
                    translation = _dbContext.Set<Translation>()
                        .FirstOrDefault(t => t.Key == key && t.Culture == parentCulture);

                    if (translation != null)
                        return translation.Value;
                }
            }

            // Try default culture
            if (culture != _settings.DefaultCulture)
            {
                translation = _dbContext.Set<Translation>()
                    .FirstOrDefault(t => t.Key == key && t.Culture == _settings.DefaultCulture);

                if (translation != null)
                    return translation.Value;
            }

            // Log missing translation
            _logger.LogWarning("Translation missing for key: {Key}, culture: {Culture}", key, culture);

            // Create missing translation record if configured
            if (_settings.ThrowOnMissingTranslation)
                throw new KeyNotFoundException($"Translation not found for key: {key}, culture: {culture}");

            return key;
        }) ?? key;
    }

    public IDictionary<string, string> GetAllStrings(string culture)
    {
        var cacheKey = $"translations_{culture}";

        return _cache.GetOrCreate(cacheKey, entry =>
        {
            entry.SetOptions(_cacheOptions);

            return _dbContext.Set<Translation>()
                .Where(t => t.Culture == culture)
                .ToDictionary(t => t.Key, t => t.Value);
        }) ?? new Dictionary<string, string>();
    }

    public CultureInfo GetCurrentCulture() => CultureInfo.CurrentCulture;

    public string[] GetSupportedCultures() => _settings.SupportedCultures;
}