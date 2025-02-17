using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SharedLibrary.Configuration;
using SharedLibrary.Interfaces;
using SharedLibrary.Services;

namespace SharedLibrary.Extensions;

/// <summary>
/// Extension methods for configuring Redis services
/// </summary>
public static class RedisExtensions
{
    /// <summary>
    /// Adds Redis cache services to the service collection
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configuration">The configuration instance</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddRedisCache(this IServiceCollection services, IConfiguration configuration)
    {
        // Register Redis settings
        services.Configure<RedisSettings>(configuration.GetSection(RedisSettings.SectionName));

        // Register Redis cache service as singleton
        services.AddSingleton<IRedisCache, RedisCache>();

        return services;
    }

    /// <summary>
    /// Adds Redis cache services to the service collection with custom settings
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="settings">The Redis settings</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddRedisCache(this IServiceCollection services, RedisSettings settings)
    {
        // Register Redis settings
        services.Configure<RedisSettings>(options =>
        {
            options.ConnectionString = settings.ConnectionString;
            options.InstanceName = settings.InstanceName;
            options.DefaultTtlMinutes = settings.DefaultTtlMinutes;
            options.UseSsl = settings.UseSsl;
            options.RetryCount = settings.RetryCount;
            options.ConnectTimeout = settings.ConnectTimeout;
            options.AbortOnConnectFail = settings.AbortOnConnectFail;
            options.AllowAdmin = settings.AllowAdmin;
            options.Database = settings.Database;
        });

        // Register Redis cache service as singleton
        services.AddSingleton<IRedisCache, RedisCache>();

        return services;
    }
}