using Google.Cloud.Translation.V2;
using Microsoft.Extensions.Options;
using TranslationService.Configuration;
using TranslationService.Interfaces;

namespace TranslationService.Services.GoogleTranslate;

/// <summary>
/// Implementation of Google Cloud Translation API provider
/// </summary>
public class GoogleTranslateProvider : ITranslationProvider
{
    private readonly TranslationClient _client;
    private readonly GoogleTranslateSettings _settings;
    private readonly ILogger<GoogleTranslateProvider> _logger;
    private IList<string>? _supportedLanguages;

    public string ProviderName => "Google Cloud Translation";

    public GoogleTranslateProvider(
        IOptions<GoogleTranslateSettings> settings,
        ILogger<GoogleTranslateProvider> logger)
    {
        _settings = settings.Value;
        _logger = logger;

        // Initialize the translation client
        _client = TranslationClient.Create();
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<string>> GetSupportedLanguagesAsync()
    {
        try
        {
            if (_supportedLanguages != null)
                return _supportedLanguages;

            var languages = await Task.FromResult(_client.ListLanguages());
            _supportedLanguages = languages.Select(l => l.Code).ToList();
            return _supportedLanguages;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting supported languages from Google Translate");
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<string> TranslateAsync(string text, string sourceLanguage, string targetLanguage)
    {
        try
        {
            var response = await Task.FromResult(_client.TranslateText(
                text: text,
                targetLanguage: targetLanguage,
                sourceLanguage: sourceLanguage));

            return response.TranslatedText;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error translating text with Google Translate. Source: {Source}, Target: {Target}",
                sourceLanguage, targetLanguage);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<IDictionary<string, string>> TranslateBatchAsync(
        IEnumerable<string> texts,
        string sourceLanguage,
        string targetLanguage)
    {
        try
        {
            var textList = texts.ToList();
            var results = new Dictionary<string, string>();

            // Process in batches according to settings
            for (int i = 0; i < textList.Count; i += _settings.MaxBatchSize)
            {
                var batch = textList.Skip(i).Take(_settings.MaxBatchSize);
                foreach (var text in batch)
                {
                    var response = await Task.FromResult(_client.TranslateText(
                        text: text,
                        targetLanguage: targetLanguage,
                        sourceLanguage: sourceLanguage));
                    results[text] = response.TranslatedText;
                }
            }

            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error batch translating texts with Google Translate. Source: {Source}, Target: {Target}",
                sourceLanguage, targetLanguage);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<string> DetectLanguageAsync(string text)
    {
        try
        {
            var detection = await Task.FromResult(_client.DetectLanguage(text));
            return detection.Language;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error detecting language with Google Translate");
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<bool> IsLanguageSupportedAsync(string languageCode)
    {
        var languages = await GetSupportedLanguagesAsync();
        return languages.Contains(languageCode);
    }
}