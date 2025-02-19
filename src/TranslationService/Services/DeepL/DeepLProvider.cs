using DeepL;
using DeepL.Model;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Polly;
using Polly.RateLimit;
using TranslationService.Configuration;
using TranslationService.Interfaces;

namespace TranslationService.Services.DeepL;

/// <summary>
/// Implementation of DeepL API provider
/// </summary>
public class DeepLProvider : ITranslationProvider, IDisposable
{
    private readonly ITranslator _client;
    private readonly DeepLSettings _settings;
    private readonly ILogger<DeepLProvider> _logger;
    private readonly IMemoryCache _cache;
    private readonly AsyncPolicy _retryPolicy;
    private readonly AsyncPolicy _rateLimitPolicy;
    private IList<string>? _supportedLanguages;

    private const string CacheKeyPrefix = "DeepL_";
    private const string LanguagesCacheKey = CacheKeyPrefix + "Languages";
    private static readonly TimeSpan DefaultCacheTime = TimeSpan.FromHours(24);

    public string ProviderName => "DeepL";

    public DeepLProvider(
        IOptions<DeepLSettings> settings,
        ILogger<DeepLProvider> logger,
        IMemoryCache cache)
    {
        _settings = settings.Value;
        _logger = logger;
        _cache = cache;

        // Initialize the translation client
        var options = new TranslatorOptions
        {
            ServerUrl = _settings.UseFreeTier ? "https://api-free.deepl.com" : "https://api.deepl.com",
            MaximumNetworkRetries = _settings.EnableRetries ? _settings.MaxRetries : 0
        };

        _client = new Translator(_settings.ApiKey, options);

        // Configure retry policy with exponential backoff
        _retryPolicy = Policy
            .Handle<Exception>(ex => IsTransientException(ex))
            .WaitAndRetryAsync(
                _settings.MaxRetries,
                retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                onRetry: (ex, timeSpan, retryCount, context) =>
                {
                    _logger.LogWarning(ex,
                        "Retry {RetryCount} after {Delay}s due to {Error}",
                        retryCount, timeSpan.TotalSeconds, ex.Message);
                });

        // Configure rate limiting policy
        _rateLimitPolicy = Policy.RateLimitAsync(
            numberOfExecutions: 100, // Adjust based on your quota
            per: TimeSpan.FromMinutes(1),
            onRejected: (context) =>
            {
                _logger.LogWarning("Rate limit exceeded. Request rejected.");
                return Task.CompletedTask;
            });
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<string>> GetSupportedLanguagesAsync()
    {
        if (_supportedLanguages != null)
            return _supportedLanguages;

        return await _cache.GetOrCreateAsync(LanguagesCacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = DefaultCacheTime;

            try
            {
                var sourceLanguages = await ExecuteWithPolicies(() =>
                    _client.GetSourceLanguagesAsync());

                _supportedLanguages = sourceLanguages
                    .Select(l => l.Code.ToLower())
                    .ToList();

                return _supportedLanguages;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting supported languages");
                throw;
            }
        });
    }

    /// <inheritdoc/>
    public async Task<string> TranslateAsync(string text, string sourceLanguage, string targetLanguage)
    {
        var cacheKey = GetTranslationCacheKey(text, sourceLanguage, targetLanguage);
        return await _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = DefaultCacheTime;

            try
            {
                var options = new TextTranslateOptions
                {
                    PreserveFormatting = _settings.PreserveFormatting == "1",
                    SplitSentences = ParseSplitSentences(_settings.SplitSentences),
                    Formality = _settings.Formality ? Formality.More : Formality.Default,
                    GlossaryId = _settings.UseGlossary ? _settings.GlossaryId : null,
                    TagHandling = ParseTagHandling(_settings.TagHandling),
                    IgnoreTags = _settings.IgnoreTags
                };

                var response = await ExecuteWithPolicies(() =>
                    _client.TranslateTextAsync(
                        text,
                        sourceLanguage == "auto" ? null : sourceLanguage,
                        targetLanguage,
                        options));

                return response.Text;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error translating text. Source: {Source}, Target: {Target}",
                    sourceLanguage, targetLanguage);
                throw;
            }
        });
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
                var batch = textList.Skip(i).Take(_settings.MaxBatchSize).ToArray();
                var options = new TextTranslateOptions
                {
                    PreserveFormatting = _settings.PreserveFormatting == "1",
                    SplitSentences = ParseSplitSentences(_settings.SplitSentences),
                    Formality = _settings.Formality ? Formality.More : Formality.Default,
                    GlossaryId = _settings.UseGlossary ? _settings.GlossaryId : null,
                    TagHandling = ParseTagHandling(_settings.TagHandling),
                    IgnoreTags = _settings.IgnoreTags
                };

                var response = await ExecuteWithPolicies(() =>
                    _client.TranslateTextAsync(
                        batch,
                        sourceLanguage == "auto" ? null : sourceLanguage,
                        targetLanguage,
                        options));

                for (int j = 0; j < batch.Length; j++)
                {
                    results[batch[j]] = response[j].Text;
                }
            }

            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error batch translating texts. Source: {Source}, Target: {Target}",
                sourceLanguage, targetLanguage);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<string> DetectLanguageAsync(string text)
    {
        var cacheKey = $"{CacheKeyPrefix}Detect_{text.GetHashCode()}";
        return await _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = DefaultCacheTime;

            try
            {
                var response = await ExecuteWithPolicies(() =>
                    _client.DetectLanguageAsync(text));

                return response.Language.ToLower();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error detecting language");
                throw;
            }
        });
    }

    /// <inheritdoc/>
    public async Task<bool> IsLanguageSupportedAsync(string languageCode)
    {
        var languages = await GetSupportedLanguagesAsync();
        return languages.Contains(languageCode.ToLower());
    }

    private async Task<T> ExecuteWithPolicies<T>(Func<Task<T>> action)
    {
        return await _rateLimitPolicy
            .WrapAsync(_retryPolicy)
            .ExecuteAsync(action);
    }

    private static string GetTranslationCacheKey(string text, string sourceLanguage, string targetLanguage)
    {
        return $"{CacheKeyPrefix}Trans_{text.GetHashCode()}_{sourceLanguage}_{targetLanguage}";
    }

    private static bool IsTransientException(Exception ex)
    {
        return ex switch
        {
            DeepLException dle => IsTransientDeepLError(dle),
            HttpRequestException => true,
            TimeoutException => true,
            TaskCanceledException => true,
            _ => false
        };
    }

    private static bool IsTransientDeepLError(DeepLException ex)
    {
        // Check for specific DeepL error types that are transient
        return ex.ErrorCode switch
        {
            "quota_exceeded" => true, // Rate limiting
            "too_many_requests" => true, // Rate limiting
            "service_unavailable" => true, // Server error
            _ => false
        };
    }

    private static SplitSentences ParseSplitSentences(string value) => value switch
    {
        "0" => SplitSentences.None,
        "nonewlines" => SplitSentences.NoNewlines,
        _ => SplitSentences.All
    };

    private static TagHandling? ParseTagHandling(string? value) => value?.ToLower() switch
    {
        "xml" => DeepL.Model.TagHandling.Xml,
        "html" => DeepL.Model.TagHandling.Html,
        _ => null
    };

    public void Dispose()
    {
        _client?.Dispose();
        GC.SuppressFinalize(this);
    }
}