using Azure;
using Azure.AI.Translation.Text;
using Azure.Core;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Polly;
using Polly.RateLimit;
using TranslationService.Configuration;
using TranslationService.Interfaces;

namespace TranslationService.Services.AzureTranslate;

/// <summary>
/// Implementation of Azure Translator API provider
/// </summary>
public class AzureTranslateProvider : ITranslationProvider, IDisposable
{
    private readonly TextTranslationClient _client;
    private readonly AzureTranslateSettings _settings;
    private readonly ILogger<AzureTranslateProvider> _logger;
    private readonly IMemoryCache _cache;
    private readonly AsyncPolicy _retryPolicy;
    private readonly AsyncPolicy _rateLimitPolicy;
    private IList<string>? _supportedLanguages;

    private const string CacheKeyPrefix = "AzureTranslate_";
    private const string LanguagesCacheKey = CacheKeyPrefix + "Languages";
    private static readonly TimeSpan DefaultCacheTime = TimeSpan.FromHours(24);

    public string ProviderName => "Azure Translator";

    public AzureTranslateProvider(
        IOptions<AzureTranslateSettings> settings,
        ILogger<AzureTranslateProvider> logger,
        IMemoryCache cache)
    {
        _settings = settings.Value;
        _logger = logger;
        _cache = cache;

        // Initialize the translation client
        var credentials = new AzureKeyCredential(_settings.SubscriptionKey);
        _client = new TextTranslationClient(
            credentials,
            _settings.Region,
            new TextTranslationClientOptions
            {
                Transport = new HttpClientTransport(new HttpClient
                {
                    Timeout = TimeSpan.FromSeconds(_settings.TimeoutSeconds)
                })
            });

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
                var response = await ExecuteWithPolicies(() =>
                    _client.GetLanguagesAsync(scope: "translation"));

                _supportedLanguages = response.Value.Translation
                    .Select(l => l.Key)
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
                var options = new TranslateTextOptions
                {
                    FromLanguage = sourceLanguage,
                    ToLanguage = targetLanguage,
                    IncludeSentenceLength = _settings.IncludeSentenceLength,
                    IncludeAlignment = _settings.IncludeAlignment,
                    ProfanityAction = _settings.FilterProfanity ? ProfanityAction.Marked : ProfanityAction.NoAction,
                    Category = _settings.Category
                };

                var response = await ExecuteWithPolicies(() =>
                    _client.TranslateAsync(text, options));

                var translation = response.Value.FirstOrDefault();
                return translation?.Translations.FirstOrDefault()?.Text ?? text;
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
                var options = new TranslateTextOptions
                {
                    FromLanguage = sourceLanguage,
                    ToLanguage = targetLanguage,
                    IncludeSentenceLength = _settings.IncludeSentenceLength,
                    IncludeAlignment = _settings.IncludeAlignment,
                    ProfanityAction = _settings.FilterProfanity ? ProfanityAction.Marked : ProfanityAction.NoAction,
                    Category = _settings.Category
                };

                var response = await ExecuteWithPolicies(() =>
                    _client.TranslateAsync(batch, options));

                for (int j = 0; j < batch.Length; j++)
                {
                    var translation = response.Value.ElementAtOrDefault(j)
                        ?.Translations.FirstOrDefault()?.Text ?? batch[j];
                    results[batch[j]] = translation;
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

                var detection = response.Value.FirstOrDefault();
                return detection?.Language ?? "und"; // "und" for undefined
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
        return languages.Contains(languageCode);
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
            RequestFailedException rfe => IsTransientStatusCode(rfe.Status),
            HttpRequestException => true,
            TimeoutException => true,
            TaskCanceledException => true,
            _ => false
        };
    }

    private static bool IsTransientStatusCode(int statusCode)
    {
        return statusCode is >= 500 or 429; // Server errors or rate limiting
    }

    public void Dispose()
    {
        _client?.Dispose();
        GC.SuppressFinalize(this);
    }
}