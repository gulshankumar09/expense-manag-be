using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SharedLibrary.Configuration;

namespace SharedLibrary.Extensions;

/// <summary>
/// Extension methods for configuring CORS
/// </summary>
public static class CorsExtensions
{
    /// <summary>
    /// Adds and configures CORS services with settings from configuration
    /// </summary>
    /// <param name="services">The IServiceCollection to add services to</param>
    /// <param name="configuration">The configuration containing CORS settings</param>
    /// <returns>The IServiceCollection for chaining</returns>
    public static IServiceCollection AddCustomCors(this IServiceCollection services, IConfiguration configuration)
    {
        // Register CORS settings
        services.Configure<CorsSettings>(configuration.GetSection("Cors"));
        var corsSettings = configuration.GetSection("Cors").Get<CorsSettings>();

        if (corsSettings == null)
        {
            throw new InvalidOperationException("CORS settings are not properly configured.");
        }

        services.AddCors(options =>
        {
            options.AddPolicy(corsSettings.PolicyName, builder =>
            {
                // Configure origins
                if (corsSettings.AllowedOrigins?.Length > 0)
                {
                    if (corsSettings.AllowedOrigins.Contains("*"))
                    {
                        builder.AllowAnyOrigin();
                    }
                    else
                    {
                        builder.WithOrigins(corsSettings.AllowedOrigins);
                    }
                }
                else
                {
                    builder.AllowAnyOrigin();
                }

                // Configure methods
                if (corsSettings.AllowedMethods?.Length > 0)
                {
                    if (corsSettings.AllowedMethods.Contains("*"))
                    {
                        builder.AllowAnyMethod();
                    }
                    else
                    {
                        builder.WithMethods(corsSettings.AllowedMethods);
                    }
                }
                else
                {
                    builder.AllowAnyMethod();
                }

                // Configure headers
                if (corsSettings.AllowedHeaders?.Length > 0)
                {
                    if (corsSettings.AllowedHeaders.Contains("*"))
                    {
                        builder.AllowAnyHeader();
                    }
                    else
                    {
                        builder.WithHeaders(corsSettings.AllowedHeaders);
                    }
                }
                else
                {
                    builder.AllowAnyHeader();
                }

                // Configure credentials
                if (corsSettings.AllowCredentials)
                {
                    builder.AllowCredentials();
                }

                // Configure preflight caching
                if (corsSettings.PreflightMaxAge > 0)
                {
                    builder.SetPreflightMaxAge(TimeSpan.FromSeconds(corsSettings.PreflightMaxAge));
                }
            });
        });

        return services;
    }

    /// <summary>
    /// Adds and configures CORS services with default development settings
    /// </summary>
    /// <param name="services">The IServiceCollection to add services to</param>
    /// <returns>The IServiceCollection for chaining</returns>
    public static IServiceCollection AddDevelopmentCors(this IServiceCollection services)
    {
        var developmentSettings = new CorsSettings
        {
            PolicyName = "Development",
            AllowedOrigins = new[] {
                "http://localhost:3000",
                "https://localhost:3000",
                "http://localhost:4200",
                "https://localhost:4200",
                "http://localhost:5173",
                "https://localhost:5173",
                "http://localhost:8081"
            },
            AllowedMethods = new[] { "GET", "POST", "PUT", "DELETE", "OPTIONS", "PATCH" },
            AllowedHeaders = new[] { "Content-Type", "Authorization", "X-Requested-With" },
            AllowCredentials = true,
            PreflightMaxAge = 600 // 10 minutes
        };

        services.AddCors(options =>
        {
            options.AddPolicy(developmentSettings.PolicyName, builder =>
            {
                builder.WithOrigins(developmentSettings.AllowedOrigins)
                       .WithMethods(developmentSettings.AllowedMethods)
                       .WithHeaders(developmentSettings.AllowedHeaders)
                       .AllowCredentials()
                       .SetPreflightMaxAge(TimeSpan.FromSeconds(developmentSettings.PreflightMaxAge));
            });
        });

        return services;
    }

    /// <summary>
    /// Adds and configures CORS services with default production settings
    /// </summary>
    /// <param name="services">The IServiceCollection to add services to</param>
    /// <param name="configuration">The configuration containing CORS settings</param>
    /// <returns>The IServiceCollection for chaining</returns>
    public static IServiceCollection AddProductionCors(this IServiceCollection services, IConfiguration configuration)
    {
        return services.AddCustomCors(configuration);
    }
}