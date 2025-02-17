using System.Text.Json;
using SharedLibrary.Interfaces;

namespace SharedLibrary.Utility;

/// <summary>
/// Utility methods for common Redis caching patterns
/// </summary>
public static class RedisCacheUtility
{
    /// <summary>
    /// Gets a cached item or creates it if it doesn't exist, with sliding expiration
    /// </summary>
    public static async Task<T> GetOrCreateWithSlidingExpirationAsync<T>(
        this IRedisCache cache,
        string key,
        Func<Task<T>> factory,
        TimeSpan slidingExpiration)
    {
        var value = await cache.GetAsync<T>(key);
        if (value != null)
        {
            // Reset expiration on access
            await cache.SetAsync(key, value, (int)slidingExpiration.TotalMinutes);
            return value;
        }

        value = await factory();
        await cache.SetAsync(key, value, (int)slidingExpiration.TotalMinutes);
        return value;
    }

    /// <summary>
    /// Gets a cached item with type-specific key prefix
    /// </summary>
    public static Task<T?> GetByTypeAsync<T>(this IRedisCache cache, string key)
        => cache.GetAsync<T>($"{typeof(T).Name}:{key}");

    /// <summary>
    /// Sets a cached item with type-specific key prefix
    /// </summary>
    public static Task SetByTypeAsync<T>(this IRedisCache cache, string key, T value, int? expirationMinutes = null)
        => cache.SetAsync($"{typeof(T).Name}:{key}", value, expirationMinutes);

    /// <summary>
    /// Gets or sets a collection of items with pagination support
    /// </summary>
    public static async Task<(IEnumerable<T> Items, int TotalCount)> GetOrSetPagedAsync<T>(
        this IRedisCache cache,
        string key,
        int pageNumber,
        int pageSize,
        Func<Task<(IEnumerable<T> Items, int TotalCount)>> factory,
        int? expirationMinutes = null)
    {
        var cacheKey = $"{key}:page_{pageNumber}_size_{pageSize}";
        var result = await cache.GetAsync<PagedCacheResult<T>>(cacheKey);

        if (result != null)
            return (result.Items, result.TotalCount);

        var (items, totalCount) = await factory();
        result = new PagedCacheResult<T>(items, totalCount);
        await cache.SetAsync(cacheKey, result, expirationMinutes);

        return (items, totalCount);
    }

    /// <summary>
    /// Caches the result of an operation with error handling
    /// </summary>
    public static async Task<T?> GetOrSetWithErrorHandlingAsync<T>(
        this IRedisCache cache,
        string key,
        Func<Task<T>> factory,
        Func<Exception, Task<T?>>? errorHandler = null,
        int? expirationMinutes = null)
    {
        try
        {
            return await cache.GetOrSetAsync(key, factory, expirationMinutes);
        }
        catch (Exception ex)
        {
            if (errorHandler != null)
                return await errorHandler(ex);
            throw;
        }
    }

    /// <summary>
    /// Gets or sets a collection with automatic batch processing
    /// </summary>
    public static async Task<IEnumerable<T>> GetOrSetBatchAsync<T>(
        this IRedisCache cache,
        IEnumerable<string> keys,
        Func<IEnumerable<string>, Task<IDictionary<string, T>>> factory,
        int? expirationMinutes = null)
    {
        var keysList = keys.ToList();
        var cachedValues = await cache.GetAllAsync<T>(keysList);
        var missingKeys = keysList.Where(k => !cachedValues.ContainsKey(k) || cachedValues[k] == null).ToList();

        if (missingKeys.Any())
        {
            var newValues = await factory(missingKeys);
            if (newValues.Any())
            {
                await cache.SetAllAsync(newValues, expirationMinutes);
                foreach (var (key, value) in newValues)
                {
                    cachedValues[key] = value;
                }
            }
        }

        return keysList.Select(k => cachedValues.TryGetValue(k, out var value) ? value : default)
                      .Where(v => v != null)!;
    }

    /// <summary>
    /// Invalidates all cache entries with the given prefix
    /// </summary>
    public static async Task InvalidateByPrefixAsync(
        this IRedisCache cache,
        string prefix,
        string channel = "cache:invalidation")
    {
        // Publish invalidation message
        var message = new CacheInvalidationMessage(prefix, DateTime.UtcNow);
        await cache.PublishAsync(channel, message);
    }

    /// <summary>
    /// Sets up cache invalidation subscription
    /// </summary>
    public static Task SubscribeToCacheInvalidationAsync(
        this IRedisCache cache,
        Action<string, DateTime> invalidationHandler,
        string channel = "cache:invalidation")
    {
        return cache.SubscribeAsync<CacheInvalidationMessage>(channel, async message =>
        {
            await Task.Run(() => invalidationHandler(message.Prefix, message.Timestamp));
        });
    }

    #region Rate Limiting

    /// <summary>
    /// Checks if a rate limit has been exceeded
    /// </summary>
    /// <param name="cache">The Redis cache</param>
    /// <param name="key">The rate limit key (e.g., "rate:ip:127.0.0.1")</param>
    /// <param name="limit">Maximum number of requests</param>
    /// <param name="windowSeconds">Time window in seconds</param>
    /// <returns>Tuple of (bool exceeded, int remaining, TimeSpan reset)</returns>
    public static async Task<(bool Exceeded, int Remaining, TimeSpan Reset)> CheckRateLimitAsync(
        this IRedisCache cache,
        string key,
        int limit,
        int windowSeconds)
    {
        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var windowStart = now - windowSeconds;

        // Get current count within window
        var count = await cache.GetAsync<int>($"ratelimit:{key}");
        if (count == 0)
        {
            await cache.SetAsync($"ratelimit:{key}", 1, windowSeconds / 60);
            return (false, limit - 1, TimeSpan.FromSeconds(windowSeconds));
        }

        if (count >= limit)
        {
            var ttl = await cache.GetTtlAsync($"ratelimit:{key}");
            return (true, 0, ttl);
        }

        await cache.IncrementAsync($"ratelimit:{key}");
        var remaining = limit - count - 1;
        var reset = await cache.GetTtlAsync($"ratelimit:{key}");

        return (false, remaining, reset);
    }

    #endregion

    #region Distributed Locking

    /// <summary>
    /// Acquires a distributed lock
    /// </summary>
    /// <param name="cache">The Redis cache</param>
    /// <param name="lockKey">The lock key</param>
    /// <param name="lockToken">Unique token for this lock instance</param>
    /// <param name="expirySeconds">Lock expiry in seconds</param>
    /// <returns>True if lock was acquired</returns>
    public static async Task<bool> AcquireLockAsync(
        this IRedisCache cache,
        string lockKey,
        string lockToken,
        int expirySeconds)
    {
        return await cache.SetAsync($"lock:{lockKey}", lockToken, expirySeconds / 60, onlyIfNotExists: true);
    }

    /// <summary>
    /// Releases a distributed lock
    /// </summary>
    /// <param name="cache">The Redis cache</param>
    /// <param name="lockKey">The lock key</param>
    /// <param name="lockToken">Token used when acquiring the lock</param>
    /// <returns>True if lock was released</returns>
    public static async Task<bool> ReleaseLockAsync(
        this IRedisCache cache,
        string lockKey,
        string lockToken)
    {
        var storedToken = await cache.GetAsync<string>($"lock:{lockKey}");
        if (storedToken == lockToken)
        {
            await cache.RemoveAsync($"lock:{lockKey}");
            return true;
        }
        return false;
    }

    #endregion

    #region Atomic Counters

    /// <summary>
    /// Increments a counter atomically
    /// </summary>
    /// <param name="cache">The Redis cache</param>
    /// <param name="key">Counter key</param>
    /// <param name="increment">Amount to increment (default: 1)</param>
    /// <param name="expirationMinutes">Optional expiration time</param>
    /// <returns>New counter value</returns>
    public static async Task<long> IncrementCounterAsync(
        this IRedisCache cache,
        string key,
        long increment = 1,
        int? expirationMinutes = null)
    {
        var value = await cache.IncrementAsync($"counter:{key}", increment);
        if (expirationMinutes.HasValue)
        {
            await cache.SetExpirationAsync($"counter:{key}", expirationMinutes.Value);
        }
        return value;
    }

    /// <summary>
    /// Decrements a counter atomically
    /// </summary>
    /// <param name="cache">The Redis cache</param>
    /// <param name="key">Counter key</param>
    /// <param name="decrement">Amount to decrement (default: 1)</param>
    /// <param name="expirationMinutes">Optional expiration time</param>
    /// <returns>New counter value</returns>
    public static async Task<long> DecrementCounterAsync(
        this IRedisCache cache,
        string key,
        long decrement = 1,
        int? expirationMinutes = null)
    {
        var value = await cache.DecrementAsync($"counter:{key}", decrement);
        if (expirationMinutes.HasValue)
        {
            await cache.SetExpirationAsync($"counter:{key}", expirationMinutes.Value);
        }
        return value;
    }

    #endregion

    #region Cache Tags

    /// <summary>
    /// Sets a value in the cache with tags for bulk operations
    /// </summary>
    /// <typeparam name="T">Type of the value</typeparam>
    /// <param name="cache">The Redis cache</param>
    /// <param name="key">Cache key</param>
    /// <param name="value">Value to cache</param>
    /// <param name="tags">Tags to associate with this cache entry</param>
    /// <param name="expirationMinutes">Optional expiration time</param>
    public static async Task SetWithTagsAsync<T>(
        this IRedisCache cache,
        string key,
        T value,
        string[] tags,
        int? expirationMinutes = null)
    {
        await cache.SetAsync(key, value, expirationMinutes);
        foreach (var tag in tags)
        {
            await cache.SetAddAsync($"tag:{tag}", key);
        }
    }

    /// <summary>
    /// Invalidates all cache entries with the specified tag
    /// </summary>
    /// <param name="cache">The Redis cache</param>
    /// <param name="tag">Tag to invalidate</param>
    public static async Task InvalidateByTagAsync(
        this IRedisCache cache,
        string tag)
    {
        var keys = await cache.SetMembersAsync($"tag:{tag}");
        if (keys.Any())
        {
            await cache.RemoveAllAsync(keys);
            await cache.RemoveAsync($"tag:{tag}");
        }
    }

    #endregion

    #region Hierarchical Cache

    /// <summary>
    /// Sets a value in the cache with hierarchical key support
    /// </summary>
    /// <typeparam name="T">Type of the value</typeparam>
    /// <param name="cache">The Redis cache</param>
    /// <param name="key">Cache key</param>
    /// <param name="value">Value to cache</param>
    /// <param name="parentKey">Parent key for hierarchical relationships</param>
    /// <param name="expirationMinutes">Optional expiration time</param>
    public static async Task SetHierarchicalAsync<T>(
        this IRedisCache cache,
        string key,
        T value,
        string parentKey,
        int? expirationMinutes = null)
    {
        await cache.SetAsync(key, value, expirationMinutes);
        await cache.SetAddAsync($"hierarchy:{parentKey}", key);
    }

    /// <summary>
    /// Invalidates a cache entry and all its children
    /// </summary>
    /// <param name="cache">The Redis cache</param>
    /// <param name="parentKey">Parent key to invalidate</param>
    public static async Task InvalidateHierarchyAsync(
        this IRedisCache cache,
        string parentKey)
    {
        var childKeys = await cache.SetMembersAsync($"hierarchy:{parentKey}");
        foreach (var childKey in childKeys)
        {
            await cache.InvalidateHierarchyAsync(childKey);
        }
        await cache.RemoveAsync(parentKey);
        await cache.RemoveAsync($"hierarchy:{parentKey}");
    }

    #endregion
}

/// <summary>
/// Helper class for storing paged results in cache
/// </summary>
internal class PagedCacheResult<T>
{
    public IEnumerable<T> Items { get; }
    public int TotalCount { get; }

    public PagedCacheResult(IEnumerable<T> items, int totalCount)
    {
        Items = items;
        TotalCount = totalCount;
    }
}

/// <summary>
/// Message for cache invalidation
/// </summary>
internal record CacheInvalidationMessage(string Prefix, DateTime Timestamp);