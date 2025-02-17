namespace SharedLibrary.Interfaces;

/// <summary>
/// Interface for Redis cache operations
/// </summary>
public interface IRedisCache
{
    /// <summary>
    /// Gets a value from the cache
    /// </summary>
    /// <typeparam name="T">The type of the value</typeparam>
    /// <param name="key">The cache key</param>
    /// <returns>The cached value or default if not found</returns>
    Task<T?> GetAsync<T>(string key);

    /// <summary>
    /// Sets a value in the cache
    /// </summary>
    /// <typeparam name="T">The type of the value</typeparam>
    /// <param name="key">The cache key</param>
    /// <param name="value">The value to cache</param>
    /// <param name="expirationMinutes">Optional expiration time in minutes</param>
    Task SetAsync<T>(string key, T value, int? expirationMinutes = null);

    /// <summary>
    /// Removes a value from the cache
    /// </summary>
    /// <param name="key">The cache key</param>
    Task RemoveAsync(string key);

    /// <summary>
    /// Checks if a key exists in the cache
    /// </summary>
    /// <param name="key">The cache key</param>
    /// <returns>True if the key exists, false otherwise</returns>
    Task<bool> ExistsAsync(string key);

    /// <summary>
    /// Gets or sets a value in the cache
    /// </summary>
    /// <typeparam name="T">The type of the value</typeparam>
    /// <param name="key">The cache key</param>
    /// <param name="factory">Function to create the value if not found</param>
    /// <param name="expirationMinutes">Optional expiration time in minutes</param>
    Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, int? expirationMinutes = null);

    /// <summary>
    /// Removes multiple keys from the cache
    /// </summary>
    /// <param name="keys">The cache keys to remove</param>
    Task RemoveAllAsync(IEnumerable<string> keys);

    /// <summary>
    /// Gets multiple values from the cache
    /// </summary>
    /// <typeparam name="T">The type of the values</typeparam>
    /// <param name="keys">The cache keys</param>
    /// <returns>Dictionary of keys and their values</returns>
    Task<IDictionary<string, T?>> GetAllAsync<T>(IEnumerable<string> keys);

    /// <summary>
    /// Sets multiple values in the cache
    /// </summary>
    /// <typeparam name="T">The type of the values</typeparam>
    /// <param name="keyValues">Dictionary of keys and values to cache</param>
    /// <param name="expirationMinutes">Optional expiration time in minutes</param>
    Task SetAllAsync<T>(IDictionary<string, T> keyValues, int? expirationMinutes = null);

    /// <summary>
    /// Publishes a message to a Redis channel
    /// </summary>
    /// <typeparam name="T">The type of the message</typeparam>
    /// <param name="channel">The channel name</param>
    /// <param name="message">The message to publish</param>
    Task PublishAsync<T>(string channel, T message);

    /// <summary>
    /// Subscribes to a Redis channel
    /// </summary>
    /// <typeparam name="T">The type of the message</typeparam>
    /// <param name="channel">The channel name</param>
    /// <param name="handler">The message handler</param>
    Task SubscribeAsync<T>(string channel, Func<T, Task> handler);

    /// <summary>
    /// Unsubscribes from a Redis channel
    /// </summary>
    /// <param name="channel">The channel name</param>
    Task UnsubscribeAsync(string channel);

    /// <summary>
    /// Gets the Time To Live (TTL) for a key
    /// </summary>
    Task<TimeSpan> GetTtlAsync(string key);

    /// <summary>
    /// Sets the expiration time for a key
    /// </summary>
    Task SetExpirationAsync(string key, int minutes);

    /// <summary>
    /// Increments a numeric value atomically
    /// </summary>
    Task<long> IncrementAsync(string key, long value = 1);

    /// <summary>
    /// Decrements a numeric value atomically
    /// </summary>
    Task<long> DecrementAsync(string key, long value = 1);

    /// <summary>
    /// Adds a member to a set
    /// </summary>
    Task SetAddAsync(string key, string value);

    /// <summary>
    /// Gets all members of a set
    /// </summary>
    Task<IEnumerable<string>> SetMembersAsync(string key);

    /// <summary>
    /// Sets a value only if the key does not exist
    /// </summary>
    Task<bool> SetAsync<T>(string key, T value, int? expirationMinutes, bool onlyIfNotExists);
}