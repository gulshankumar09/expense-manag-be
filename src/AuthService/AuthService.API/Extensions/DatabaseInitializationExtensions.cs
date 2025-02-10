using AuthService.Domain.Entities;
using AuthService.Domain.ValueObjects;
using AuthService.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Polly;

namespace AuthService.API.Extensions;

public static class DatabaseInitializationExtensions
{
    public static async Task InitializeDatabaseAsync(this IApplicationBuilder app)
    {
        using (var scope = app.ApplicationServices.CreateScope())
        {
            var services = scope.ServiceProvider;
            try
            {
                var context = services.GetRequiredService<AuthDbContext>();
                var logger = services.GetRequiredService<ILogger<AuthDbContext>>();

                logger.LogInformation("Waiting for database to be ready...");

                // Create a retry policy for database operations
                var policy = Policy
                    .Handle<Exception>()
                    .WaitAndRetryAsync(10, retryAttempt =>
                        TimeSpan.FromSeconds(Math.Min(Math.Pow(2, retryAttempt), 30)), // exponential backoff
                        (exception, timeSpan, retryCount, context) =>
                        {
                            logger.LogWarning(exception,
                                "Failed to connect to database. Retry attempt {RetryCount}. Waiting {TimeSpan} seconds",
                                retryCount,
                                timeSpan.TotalSeconds);
                        });

                await policy.ExecuteAsync(async () =>
                {
                    // Apply any pending migrations
                    logger.LogInformation("Applying pending migrations...");
                    await context.Database.MigrateAsync();
                    logger.LogInformation("Migrations applied successfully.");
                });

                logger.LogInformation("Database initialization completed successfully.");
            }
            catch (Exception ex)
            {
                var logger = services.GetRequiredService<ILogger<AuthDbContext>>();
                logger.LogError(ex, "An error occurred while initializing the database.");
                throw;
            }
        }
    }
}