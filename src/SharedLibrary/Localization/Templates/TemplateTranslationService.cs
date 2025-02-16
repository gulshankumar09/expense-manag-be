using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using SharedLibrary.Configuration;

namespace SharedLibrary.Localization.Templates;

/// <summary>
/// Service for managing and applying translation templates
/// </summary>
public class TemplateTranslationService : ITemplateTranslationService
{
    private readonly ILocalizationService _localizationService;
    private readonly IMemoryCache _cache;
    private readonly ILogger<TemplateTranslationService> _logger;
    private readonly LocalizationSettings _settings;

    public TemplateTranslationService(
        ILocalizationService localizationService,
        IMemoryCache cache,
        ILogger<TemplateTranslationService> logger,
        LocalizationSettings settings)
    {
        _localizationService = localizationService;
        _cache = cache;
        _logger = logger;
        _settings = settings;
    }

    /// <summary>
    /// Gets a formatted translation using a template and dynamic values
    /// </summary>
    public string GetTemplatedTranslation(string key, string culture, IDictionary<string, object> values)
    {
        var template = GetOrCreateTemplate(key, culture);
        return template.Format(values);
    }

    /// <summary>
    /// Gets a user-specific translation with formatted values
    /// </summary>
    public string GetUserTranslation(string key, string userId, string culture, IDictionary<string, object> values)
    {
        // Add user-specific values
        var enrichedValues = new Dictionary<string, object>(values)
        {
            ["UserId"] = userId,
            // Add more user-specific values as needed
        };

        return GetTemplatedTranslation(key, culture, enrichedValues);
    }

    private TranslationTemplate GetOrCreateTemplate(string key, string culture)
    {
        var cacheKey = $"template_{culture}_{key}";

        return _cache.GetOrCreate(cacheKey, entry =>
        {
            entry.SlidingExpiration = TimeSpan.FromHours(1);

            // Get the template text from localization service
            var templateText = _localizationService.GetLocalizedString(key, culture);

            // Create standard formatters
            var formatters = CreateStandardFormatters();

            return new TranslationTemplate(templateText, formatters);
        }) ?? throw new InvalidOperationException($"Failed to create or retrieve template for key: {key}");
    }

    private static Dictionary<string, Func<object, string>> CreateStandardFormatters()
    {
        return new Dictionary<string, Func<object, string>>
        {
            // Number formatting
            ["Number"] = value => ((decimal)value).ToString("N0"),
            ["Currency"] = value => ((decimal)value).ToString("C"),
            ["Percent"] = value => ((decimal)value).ToString("P"),

            // Date formatting
            ["Date"] = value => ((DateTime)value).ToString("d"),
            ["Time"] = value => ((DateTime)value).ToString("t"),
            ["DateTime"] = value => ((DateTime)value).ToString("g"),

            // String formatting
            ["Upper"] = value => value.ToString()!.ToUpperInvariant(),
            ["Lower"] = value => value.ToString()!.ToLowerInvariant(),

            // Add more standard formatters as needed
        };
    }
}