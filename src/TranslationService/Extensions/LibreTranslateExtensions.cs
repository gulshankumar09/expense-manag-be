using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TranslationService.Configuration;
using TranslationService.Interfaces;
using TranslationService.Services.LibreTranslate;

namespace TranslationService.Extensions;

/// <summary>
/// Extension methods for configuring LibreTranslate services
/// </summary>
public static class LibreTranslateExtensions
{
    /// <summary>
    /// Adds LibreTranslate services to the service collection
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configuration">The configuration instance</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddLibreTranslate(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Register memory cache if not already registered
        services.AddMemoryCache(options =>
        {
            options.SizeLimit = 1024; // Cache size limit in MB
            options.CompactionPercentage = 0.25; // Remove 25% of items when size limit is reached
        });

        // Register settings
        services.Configure<LibreTranslateSettings>(
            configuration.GetSection(LibreTranslateSettings.SectionName));

        // Register HTTP client
        services.AddHttpClient<ITranslationProvider, LibreTranslateProvider>((serviceProvider, client) =>
        {
            var settings = configuration
                .GetSection(LibreTranslateSettings.SectionName)
                .Get<LibreTranslateSettings>();

            if (settings != null)
            {
                var baseUrl = settings.ApiUrl;
                if (!baseUrl.EndsWith("/"))
                    baseUrl += "/";

                client.BaseAddress = new Uri(baseUrl);
                client.Timeout = TimeSpan.FromSeconds(settings.TimeoutSeconds);

                if (!string.IsNullOrEmpty(settings.ApiKey))
                {
                    client.DefaultRequestHeaders.Add("Authorization", $"ApiKey {settings.ApiKey}");
                }
            }
        })
        .ConfigurePrimaryHttpMessageHandler(() =>
        {
            var handler = new HttpClientHandler();
            var settings = configuration
                .GetSection(LibreTranslateSettings.SectionName)
                .Get<LibreTranslateSettings>();

            if (settings != null)
            {
                handler.ServerCertificateCustomValidationCallback =
                    (message, cert, chain, errors) => settings.VerifySsl;
            }

            return handler;
        });

        return services;
    }

    /// <summary>
    /// Adds LibreTranslate services to the service collection with custom settings
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="settings">The LibreTranslate settings</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddLibreTranslate(
        this IServiceCollection services,
        LibreTranslateSettings settings)
    {
        // Register memory cache if not already registered
        services.AddMemoryCache(options =>
        {
            options.SizeLimit = 1024; // Cache size limit in MB
            options.CompactionPercentage = 0.25; // Remove 25% of items when size limit is reached
        });

        // Register settings
        services.Configure<LibreTranslateSettings>(options =>
        {
            options.ApiUrl = settings.ApiUrl;
            options.ApiKey = settings.ApiKey;
            options.TimeoutSeconds = settings.TimeoutSeconds;
            options.MaxBatchSize = settings.MaxBatchSize;
            options.EnableRetries = settings.EnableRetries;
            options.MaxRetries = settings.MaxRetries;
            options.UseHttps = settings.UseHttps;
            options.VerifySsl = settings.VerifySsl;
            options.PreferFast = settings.PreferFast;
            options.IncludeSourceText = settings.IncludeSourceText;
            options.IncludeConfidence = settings.IncludeConfidence;
        });

        // Register HTTP client
        services.AddHttpClient<ITranslationProvider, LibreTranslateProvider>((serviceProvider, client) =>
        {
            var baseUrl = settings.ApiUrl;
            if (!baseUrl.EndsWith("/"))
                baseUrl += "/";

            client.BaseAddress = new Uri(baseUrl);
            client.Timeout = TimeSpan.FromSeconds(settings.TimeoutSeconds);

            if (!string.IsNullOrEmpty(settings.ApiKey))
            {
                client.DefaultRequestHeaders.Add("Authorization", $"ApiKey {settings.ApiKey}");
            }
        })
        .ConfigurePrimaryHttpMessageHandler(() =>
        {
            var handler = new HttpClientHandler();
            handler.ServerCertificateCustomValidationCallback =
                (message, cert, chain, errors) => settings.VerifySsl;
            return handler;
        });

        return services;
    }
}