using Microsoft.Extensions.Options;
using TranslationService.Configuration;
using TranslationService.Enums;
using TranslationService.Interfaces;
using TranslationService.Models;

namespace TranslationService.Services;

/// <summary>
/// A resilient translation provider that implements fallback logic
/// </summary>
public class ResilientTranslationProvider : ITranslationProvider
{
    private readonly ITranslationProviderFactory _factory;
    private readonly ILogger<ResilientTranslationProvider> _logger;
    private readonly TranslationServiceSettings _settings;
    private readonly TranslationProvider _primaryProvider;
    private readonly ITranslationRepository _repository;

    public string ProviderName => $"Resilient({_primaryProvider})";

    public ResilientTranslationProvider(
        ITranslationProviderFactory factory,
        ILogger<ResilientTranslationProvider> logger,
        IOptions<TranslationServiceSettings> settings,
        TranslationProvider primaryProvider,
        ITranslationRepository repository)
    {
        _factory = factory;
        _logger = logger;
        _settings = settings.Value;
        _primaryProvider = primaryProvider;
        _repository = repository;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<string>> GetSupportedLanguagesAsync()
    {
        var errors = new List<Exception>();
        var providers = GetProvidersInFallbackOrder();

        foreach (var provider in providers)
        {
            try
            {
                return await provider.GetSupportedLanguagesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "Provider {Provider} failed to get supported languages. Trying next provider.",
                    provider.ProviderName);
                errors.Add(ex);
            }
        }

        throw new AggregateException(
            "All translation providers failed to get supported languages.",
            errors);
    }

    /// <inheritdoc/>
    public async Task<string> TranslateAsync(string text, string sourceLanguage, string targetLanguage)
    {
        // Check if translation exists in database
        var existingTranslation = await _repository.FindTranslationAsync(text, sourceLanguage, targetLanguage);
        if (existingTranslation != null)
        {
            await _repository.UpdateTranslationUsageAsync(existingTranslation.Id);
            return existingTranslation.TranslatedText;
        }

        // If not found, try translation providers
        var errors = new List<Exception>();
        var providers = GetProvidersInFallbackOrder();

        foreach (var provider in providers)
        {
            try
            {
                var translatedText = await provider.TranslateAsync(text, sourceLanguage, targetLanguage);

                // Save successful translation to database
                await _repository.SaveTranslationAsync(new Translation
                {
                    SourceText = text,
                    TranslatedText = translatedText,
                    SourceLanguage = sourceLanguage,
                    TargetLanguage = targetLanguage,
                    Provider = _primaryProvider,
                    CreatedAt = DateTime.UtcNow,
                    LastUsedAt = DateTime.UtcNow,
                    UseCount = 1
                });

                return translatedText;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "Provider {Provider} failed to translate text. Trying next provider.",
                    provider.ProviderName);
                errors.Add(ex);
            }
        }

        throw new AggregateException(
            "All translation providers failed to translate the text.",
            errors);
    }

    /// <inheritdoc/>
    public async Task<IDictionary<string, string>> TranslateBatchAsync(
        IEnumerable<string> texts,
        string sourceLanguage,
        string targetLanguage)
    {
        var result = new Dictionary<string, string>();
        var textsToTranslate = new List<string>();
        var existingTranslations = new Dictionary<string, Translation>();

        // Check database for existing translations
        foreach (var text in texts)
        {
            var translation = await _repository.FindTranslationAsync(text, sourceLanguage, targetLanguage);
            if (translation != null)
            {
                existingTranslations[text] = translation;
                result[text] = translation.TranslatedText;
            }
            else
            {
                textsToTranslate.Add(text);
            }
        }

        // Update usage for existing translations
        foreach (var translation in existingTranslations.Values)
        {
            await _repository.UpdateTranslationUsageAsync(translation.Id);
        }

        // If all translations were found in database, return early
        if (!textsToTranslate.Any())
            return result;

        // Translate remaining texts
        var errors = new List<Exception>();
        var providers = GetProvidersInFallbackOrder();

        foreach (var provider in providers)
        {
            try
            {
                var translations = await provider.TranslateBatchAsync(textsToTranslate, sourceLanguage, targetLanguage);

                // Save successful translations to database
                foreach (var (text, translatedText) in translations)
                {
                    await _repository.SaveTranslationAsync(new Translation
                    {
                        SourceText = text,
                        TranslatedText = translatedText,
                        SourceLanguage = sourceLanguage,
                        TargetLanguage = targetLanguage,
                        Provider = _primaryProvider,
                        CreatedAt = DateTime.UtcNow,
                        LastUsedAt = DateTime.UtcNow,
                        UseCount = 1
                    });
                    result[text] = translatedText;
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "Provider {Provider} failed to batch translate texts. Trying next provider.",
                    provider.ProviderName);
                errors.Add(ex);
            }
        }

        throw new AggregateException(
            "All translation providers failed to batch translate the texts.",
            errors);
    }

    /// <inheritdoc/>
    public async Task<string> DetectLanguageAsync(string text)
    {
        var errors = new List<Exception>();
        var providers = GetProvidersInFallbackOrder();

        foreach (var provider in providers)
        {
            try
            {
                return await provider.DetectLanguageAsync(text);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "Provider {Provider} failed to detect language. Trying next provider.",
                    provider.ProviderName);
                errors.Add(ex);
            }
        }

        throw new AggregateException(
            "All translation providers failed to detect the language.",
            errors);
    }

    /// <inheritdoc/>
    public async Task<bool> IsLanguageSupportedAsync(string languageCode)
    {
        var errors = new List<Exception>();
        var providers = GetProvidersInFallbackOrder();

        foreach (var provider in providers)
        {
            try
            {
                return await provider.IsLanguageSupportedAsync(languageCode);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "Provider {Provider} failed to check language support. Trying next provider.",
                    provider.ProviderName);
                errors.Add(ex);
            }
        }

        throw new AggregateException(
            "All translation providers failed to check language support.",
            errors);
    }

    private IEnumerable<ITranslationProvider> GetProvidersInFallbackOrder()
    {
        if (!_settings.EnableFallback)
        {
            // If fallback is disabled, only use the primary provider
            yield return _factory.GetProvider(_primaryProvider);
            yield break;
        }

        // Start with the primary provider
        yield return _factory.GetProvider(_primaryProvider);

        // Then try other providers in the configured order
        foreach (var providerType in _settings.FallbackOrder)
        {
            // Skip the primary provider as we've already tried it
            if (providerType == _primaryProvider)
                continue;

            // Skip providers that aren't available
            if (!_factory.IsProviderAvailable(providerType))
                continue;

            yield return _factory.GetProvider(providerType);
        }
    }
}