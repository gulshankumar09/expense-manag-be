using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SharedLibrary.Configuration;
using SharedLibrary.Interfaces;
using SharedLibrary.Services;

namespace SharedLibrary.Extensions;

/// <summary>
/// Extension methods for configuring email services
/// </summary>
public static class EmailServiceExtensions
{
    /// <summary>
    /// Adds email service to the service collection
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configuration">The configuration instance</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddEmailService(this IServiceCollection services, IConfiguration configuration)
    {
        // Register email settings
        services.Configure<EmailSettings>(configuration.GetSection(EmailSettings.SectionName));

        // Register email service
        services.AddScoped<IEmailService, EmailService>();

        return services;
    }

}