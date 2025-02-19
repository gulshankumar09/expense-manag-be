using TranslationService.Enums;

namespace TranslationService.Interfaces;

/// <summary>
/// Factory interface for creating translation providers
/// </summary>
public interface ITranslationProviderFactory
{
    /// <summary>
    /// Gets a translation provider by type
    /// </summary>
    /// <param name="provider">The type of provider to get</param>
    /// <returns>The translation provider instance</returns>
    ITranslationProvider GetProvider(TranslationProvider provider);

    /// <summary>
    /// Gets all registered translation providers
    /// </summary>
    /// <returns>Dictionary of provider types and their instances</returns>
    IDictionary<TranslationProvider, ITranslationProvider> GetAllProviders();

    /// <summary>
    /// Gets the default translation provider
    /// </summary>
    /// <returns>The default translation provider instance</returns>
    ITranslationProvider GetDefaultProvider();

    /// <summary>
    /// Checks if a specific provider type is available
    /// </summary>
    /// <param name="provider">The type of provider to check</param>
    /// <returns>True if the provider is available</returns>
    bool IsProviderAvailable(TranslationProvider provider);
}