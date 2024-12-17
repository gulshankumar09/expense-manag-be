using AuthService.Application.Interfaces;
using AuthService.Application.Services;
using AuthService.Infrastructure.Persistence;
using AuthService.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using SharedLibrary.Infrastructure.Interceptors;
using SharedLibrary.Services;

namespace AuthService.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<AuditableEntitySaveChangesInterceptor>();

        services.AddDbContext<AuthDbContext>((sp, options) =>
        {
            var interceptor = sp.GetService<AuditableEntitySaveChangesInterceptor>();
            
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(AuthDbContext).Assembly.FullName));

            if (interceptor != null)
            {
                options.AddInterceptors(interceptor);
            }
        });

        return services;
    }

    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<IAuditService, AuditService>();
        
        // Register repositories
        services.AddScoped<IUserRepository, UserRepository>();

        // Register services
        services.AddScoped<IUserService, UserService>();

        return services;
    }
} 