using Microsoft.Extensions.Options;
using TranslationService.Configuration;
using TranslationService.Enums;
using TranslationService.Interfaces;

namespace TranslationService.Services;

/// <summary>
/// Factory implementation for managing translation providers
/// </summary>
public class TranslationProviderFactory : ITranslationProviderFactory
{
    private readonly IDictionary<TranslationProvider, ITranslationProvider> _providers;
    private readonly TranslationProvider _defaultProvider;

    public TranslationProviderFactory(
        IEnumerable<ITranslationProvider> providers,
        IOptions<TranslationServiceSettings> settings)
    {
        _providers = new Dictionary<TranslationProvider, ITranslationProvider>();
        _defaultProvider = settings.Value.DefaultProvider;

        foreach (var provider in providers)
        {
            var providerType = GetProviderType(provider.ProviderName);
            if (providerType.HasValue)
            {
                _providers[providerType.Value] = provider;
            }
        }
    }

    /// <inheritdoc/>
    public ITranslationProvider GetProvider(TranslationProvider provider)
    {
        if (_providers.TryGetValue(provider, out var translationProvider))
        {
            return translationProvider;
        }

        throw new ArgumentException($"Provider {provider} is not available.");
    }

    /// <inheritdoc/>
    public IDictionary<TranslationProvider, ITranslationProvider> GetAllProviders()
    {
        return new Dictionary<TranslationProvider, ITranslationProvider>(_providers);
    }

    /// <inheritdoc/>
    public ITranslationProvider GetDefaultProvider()
    {
        if (_providers.TryGetValue(_defaultProvider, out var provider))
        {
            return provider;
        }

        // If default provider is not available, return the first available provider
        var firstProvider = _providers.FirstOrDefault();
        if (firstProvider.Value != null)
        {
            return firstProvider.Value;
        }

        throw new InvalidOperationException("No translation providers are available.");
    }

    /// <inheritdoc/>
    public bool IsProviderAvailable(TranslationProvider provider)
    {
        return _providers.ContainsKey(provider);
    }

    private static TranslationProvider? GetProviderType(string providerName)
    {
        return providerName.ToLowerInvariant() switch
        {
            "google cloud translation" => TranslationProvider.Google,
            "azure translator" => TranslationProvider.Azure,
            "deepl" => TranslationProvider.DeepL,
            "libretranslate" => TranslationProvider.LibreTranslate,
            _ => null
        };
    }
}