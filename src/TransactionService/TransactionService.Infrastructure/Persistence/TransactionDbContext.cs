using Microsoft.EntityFrameworkCore;
using TransactionService.Domain.Entities;
using TransactionService.Infrastructure.Persistence.Configurations;

namespace TransactionService.Infrastructure.Persistence;

public class TransactionDbContext : DbContext
{
    public DbSet<Transaction> Transactions { get; set; }

    public TransactionDbContext(DbContextOptions<TransactionDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new TransactionConfiguration());
    }
} 