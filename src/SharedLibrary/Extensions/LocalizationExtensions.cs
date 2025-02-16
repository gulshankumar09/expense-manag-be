using System.Globalization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SharedLibrary.Configuration;
using SharedLibrary.Localization;

namespace SharedLibrary.Extensions;

/// <summary>
/// Extension methods for configuring localization and globalization
/// </summary>
public static class LocalizationExtensions
{
    /// <summary>
    /// Adds localization and globalization services to the service collection
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configuration">The configuration instance</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddAppLocalization(this IServiceCollection services, IConfiguration configuration)
    {
        // Register localization settings
        services.Configure<LocalizationSettings>(configuration.GetSection(LocalizationSettings.SectionName));
        var settings = configuration.GetSection(LocalizationSettings.SectionName).Get<LocalizationSettings>();

        if (settings == null)
        {
            throw new InvalidOperationException("Localization settings are not properly configured.");
        }

        // Add localization services
        services.AddLocalization(options => options.ResourcesPath = settings.ResourcesPath);

        // Register custom localization and globalization services
        services.AddSingleton<ILocalizationService, LocalizationService>();
        services.AddSingleton<IGlobalizationService, GlobalizationService>();

        // Configure supported cultures
        services.Configure<RequestLocalizationOptions>(options =>
        {
            var supportedCultures = settings.SupportedCultures
                .Select(culture => new CultureInfo(culture))
                .ToList();

            options.DefaultRequestCulture = new RequestCulture(settings.DefaultCulture);
            options.SupportedCultures = supportedCultures;
            options.SupportedUICultures = supportedCultures;

            if (settings.UseUserPreferredCulture)
            {
                options.RequestCultureProviders = new List<IRequestCultureProvider>
                {
                    new QueryStringRequestCultureProvider(),
                    new CookieRequestCultureProvider(),
                    new AcceptLanguageHeaderRequestCultureProvider()
                };
            }
            else
            {
                options.RequestCultureProviders = new List<IRequestCultureProvider>
                {
                    new QueryStringRequestCultureProvider(),
                    new CookieRequestCultureProvider()
                };
            }

            // Set default date and number formats for each culture
            foreach (var culture in supportedCultures)
            {
                if (!settings.UseInvariantCulture)
                {
                    culture.NumberFormat.CurrencyDecimalDigits = 2;
                    culture.NumberFormat.NumberDecimalDigits = 2;
                    culture.DateTimeFormat.ShortDatePattern = settings.DefaultDateFormat;
                    culture.DateTimeFormat.ShortTimePattern = settings.DefaultTimeFormat;
                }
            }
        });

        return services;
    }

    /// <summary>
    /// Adds localization and globalization middleware to the application pipeline
    /// </summary>
    /// <param name="app">The application builder</param>
    /// <returns>The application builder for chaining</returns>
    public static IApplicationBuilder UseAppLocalization(this IApplicationBuilder app)
    {
        app.UseRequestLocalization();
        return app;
    }

    /// <summary>
    /// Gets the localized string for a key using the current culture
    /// </summary>
    public static string Localize(this string key, ILocalizationService localizationService, params object[] args)
        => localizationService.GetString(key, args);

    /// <summary>
    /// Gets the localized string for a key using the specified culture
    /// </summary>
    public static string Localize(this string key, ILocalizationService localizationService, string culture, params object[] args)
        => localizationService.GetString(key, culture, args);

    /// <summary>
    /// Gets the current culture code
    /// </summary>
    public static string GetCurrentCulture(this ILocalizationService localizationService)
        => CultureInfo.CurrentCulture.Name;

    /// <summary>
    /// Gets the current UI culture code
    /// </summary>
    public static string GetCurrentUICulture(this ILocalizationService localizationService)
        => CultureInfo.CurrentUICulture.Name;

    /// <summary>
    /// Checks if a culture is supported by the application
    /// </summary>
    public static bool IsSupportedCulture(this ILocalizationService localizationService, string culture)
        => localizationService.GetSupportedCultures().Contains(culture);

    /// <summary>
    /// Gets the display name of a culture in its native language
    /// </summary>
    public static string GetCultureDisplayName(this ILocalizationService localizationService, string culture)
        => new CultureInfo(culture).NativeName;
}