using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TranslationService.Configuration;
using TranslationService.Interfaces;
using TranslationService.Services.AzureTranslate;

namespace TranslationService.Extensions;

/// <summary>
/// Extension methods for configuring Azure Translator services
/// </summary>
public static class AzureTranslateExtensions
{
    /// <summary>
    /// Adds Azure Translator services to the service collection
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configuration">The configuration instance</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddAzureTranslate(
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
        services.Configure<AzureTranslateSettings>(
            configuration.GetSection(AzureTranslateSettings.SectionName));

        // Register Azure Translator provider
        services.AddSingleton<ITranslationProvider, AzureTranslateProvider>();

        return services;
    }

    /// <summary>
    /// Adds Azure Translator services to the service collection with custom settings
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="settings">The Azure Translator settings</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddAzureTranslate(
        this IServiceCollection services,
        AzureTranslateSettings settings)
    {
        // Register memory cache if not already registered
        services.AddMemoryCache(options =>
        {
            options.SizeLimit = 1024; // Cache size limit in MB
            options.CompactionPercentage = 0.25; // Remove 25% of items when size limit is reached
        });

        // Register settings
        services.Configure<AzureTranslateSettings>(options =>
        {
            options.SubscriptionKey = settings.SubscriptionKey;
            options.Endpoint = settings.Endpoint;
            options.Region = settings.Region;
            options.TimeoutSeconds = settings.TimeoutSeconds;
            options.MaxBatchSize = settings.MaxBatchSize;
            options.EnableRetries = settings.EnableRetries;
            options.MaxRetries = settings.MaxRetries;
            options.UsePremiumTier = settings.UsePremiumTier;
            options.IncludeSentenceLength = settings.IncludeSentenceLength;
            options.IncludeAlignment = settings.IncludeAlignment;
            options.IncludeSourceText = settings.IncludeSourceText;
            options.FilterProfanity = settings.FilterProfanity;
            options.Category = settings.Category;
        });

        // Register Azure Translator provider
        services.AddSingleton<ITranslationProvider, AzureTranslateProvider>();

        return services;
    }
}