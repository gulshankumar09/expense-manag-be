using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TranslationService.Configuration;
using TranslationService.Interfaces;
using TranslationService.Services.DeepL;

namespace TranslationService.Extensions;

/// <summary>
/// Extension methods for configuring DeepL services
/// </summary>
public static class DeepLExtensions
{
    /// <summary>
    /// Adds DeepL services to the service collection
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configuration">The configuration instance</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddDeepL(
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
        services.Configure<DeepLSettings>(
            configuration.GetSection(DeepLSettings.SectionName));

        // Register DeepL provider
        services.AddSingleton<ITranslationProvider, DeepLProvider>();

        return services;
    }

    /// <summary>
    /// Adds DeepL services to the service collection with custom settings
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="settings">The DeepL settings</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddDeepL(
        this IServiceCollection services,
        DeepLSettings settings)
    {
        // Register memory cache if not already registered
        services.AddMemoryCache(options =>
        {
            options.SizeLimit = 1024; // Cache size limit in MB
            options.CompactionPercentage = 0.25; // Remove 25% of items when size limit is reached
        });

        // Register settings
        services.Configure<DeepLSettings>(options =>
        {
            options.ApiKey = settings.ApiKey;
            options.UseFreeTier = settings.UseFreeTier;
            options.TimeoutSeconds = settings.TimeoutSeconds;
            options.MaxBatchSize = settings.MaxBatchSize;
            options.EnableRetries = settings.EnableRetries;
            options.MaxRetries = settings.MaxRetries;
            options.SplitSentences = settings.SplitSentences;
            options.PreserveFormatting = settings.PreserveFormatting;
            options.Formality = settings.Formality;
            options.TagHandling = settings.TagHandling;
            options.IgnoreTags = settings.IgnoreTags;
            options.UseGlossary = settings.UseGlossary;
            options.GlossaryId = settings.GlossaryId;
        });

        // Register DeepL provider
        services.AddSingleton<ITranslationProvider, DeepLProvider>();

        return services;
    }
}