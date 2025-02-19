using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TranslationService.Configuration;
using TranslationService.Interfaces;
using TranslationService.Services.GoogleTranslate;

namespace TranslationService.Extensions;

/// <summary>
/// Extension methods for configuring Google Translate services
/// </summary>
public static class GoogleTranslateExtensions
{
    /// <summary>
    /// Adds Google Translate services to the service collection
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configuration">The configuration instance</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddGoogleTranslate(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Register settings
        services.Configure<GoogleTranslateSettings>(
            configuration.GetSection(GoogleTranslateSettings.SectionName));

        // Register Google Translate provider
        services.AddSingleton<ITranslationProvider, GoogleTranslateProvider>();

        return services;
    }

    /// <summary>
    /// Adds Google Translate services to the service collection with custom settings
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="settings">The Google Translate settings</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddGoogleTranslate(
        this IServiceCollection services,
        GoogleTranslateSettings settings)
    {
        // Register settings
        services.Configure<GoogleTranslateSettings>(options =>
        {
            options.ProjectId = settings.ProjectId;
            options.CredentialsPath = settings.CredentialsPath;
            options.CredentialsJson = settings.CredentialsJson;
            options.Location = settings.Location;
            options.UsePremiumModel = settings.UsePremiumModel;
            options.TimeoutSeconds = settings.TimeoutSeconds;
            options.MaxBatchSize = settings.MaxBatchSize;
            options.EnableRetries = settings.EnableRetries;
            options.MaxRetries = settings.MaxRetries;
        });

        // Register Google Translate provider
        services.AddSingleton<ITranslationProvider, GoogleTranslateProvider>();

        return services;
    }
}