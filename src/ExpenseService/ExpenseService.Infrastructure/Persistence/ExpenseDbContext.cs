using ExpenseService.Domain.Entities;
using ExpenseService.Infrastructure.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;

namespace ExpenseService.Infrastructure.Persistence;

public class ExpenseDbContext : DbContext
{
    public DbSet<Expense> Expenses { get; set; }

    public ExpenseDbContext(DbContextOptions<ExpenseDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new ExpenseConfiguration());
        modelBuilder.ApplyConfiguration(new ExpenseSplitConfiguration());
    }
}