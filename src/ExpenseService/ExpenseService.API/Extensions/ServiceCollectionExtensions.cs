using ExpenseService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ExpenseService.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ExpenseDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(ExpenseDbContext).Assembly.FullName)));

        return services;
    }
} 