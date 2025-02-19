namespace TranslationService.Interfaces;

/// <summary>
/// Interface for translation service providers
/// </summary>
public interface ITranslationProvider
{
    /// <summary>
    /// Gets the name of the translation provider
    /// </summary>
    string ProviderName { get; }

    /// <summary>
    /// Gets the list of supported languages
    /// </summary>
    Task<IEnumerable<string>> GetSupportedLanguagesAsync();

    /// <summary>
    /// Translates text to the target language
    /// </summary>
    /// <param name="text">Text to translate</param>
    /// <param name="sourceLanguage">Source language code (e.g., "en")</param>
    /// <param name="targetLanguage">Target language code (e.g., "es")</param>
    /// <returns>Translated text</returns>
    Task<string> TranslateAsync(string text, string sourceLanguage, string targetLanguage);

    /// <summary>
    /// Translates multiple texts to the target language
    /// </summary>
    /// <param name="texts">Texts to translate</param>
    /// <param name="sourceLanguage">Source language code (e.g., "en")</param>
    /// <param name="targetLanguage">Target language code (e.g., "es")</param>
    /// <returns>Dictionary of original texts and their translations</returns>
    Task<IDictionary<string, string>> TranslateBatchAsync(IEnumerable<string> texts, string sourceLanguage, string targetLanguage);

    /// <summary>
    /// Detects the language of the provided text
    /// </summary>
    /// <param name="text">Text to analyze</param>
    /// <returns>Detected language code</returns>
    Task<string> DetectLanguageAsync(string text);

    /// <summary>
    /// Checks if the provider supports a specific language
    /// </summary>
    /// <param name="languageCode">Language code to check</param>
    /// <returns>True if the language is supported</returns>
    Task<bool> IsLanguageSupportedAsync(string languageCode);
}