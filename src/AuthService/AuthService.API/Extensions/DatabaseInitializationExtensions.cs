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

                    // Create default roles
                    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
                    var roles = new[] { "SuperAdmin", "Admin", "User" };

                    foreach (var role in roles)
                    {
                        if (!await roleManager.RoleExistsAsync(role))
                        {
                            await roleManager.CreateAsync(new IdentityRole(role));
                            logger.LogInformation($"Created role: {role}");
                        }
                    }

                    // Create default admin user
                    var userManager = services.GetRequiredService<UserManager<User>>();
                    var adminEmail = "admin@expensesplitter.com";
                    var adminUser = await userManager.FindByEmailAsync(adminEmail);

                    if (adminUser == null)
                    {
                        adminUser = new User
                        {
                            UserName = adminEmail,
                            Email = adminEmail,
                            Name = PersonName.Create("Admin", "User"),
                            EmailConfirmed = true, // Skip email verification for admin
                            CreatedAt = DateTime.UtcNow,
                            CreatedBy = "SYSTEM",
                            IsActive = true
                        };

                        var result = await userManager.CreateAsync(adminUser, "Admin123!@#");
                        if (result.Succeeded)
                        {
                            await userManager.AddToRoleAsync(adminUser, "SuperAdmin");
                            await userManager.AddToRoleAsync(adminUser, "Admin");
                            logger.LogInformation("Created admin user and assigned roles successfully");
                        }
                        else
                        {
                            logger.LogError("Failed to create admin user: {Errors}",
                                string.Join(", ", result.Errors.Select(e => e.Description)));
                        }
                    }
                    else
                    {
                        // Add SuperAdmin role to existing admin user if they don't have it
                        if (!await userManager.IsInRoleAsync(adminUser, "SuperAdmin"))
                        {
                            await userManager.AddToRoleAsync(adminUser, "SuperAdmin");
                            logger.LogInformation("Added SuperAdmin role to existing admin user");
                        }
                    }
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