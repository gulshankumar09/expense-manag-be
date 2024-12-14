using TransactionService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace TransactionService.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<TransactionDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(TransactionDbContext).Assembly.FullName)));

        return services;
    }
} 