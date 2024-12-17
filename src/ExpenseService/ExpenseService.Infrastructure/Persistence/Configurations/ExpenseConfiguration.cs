using ExpenseService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SharedLibrary.Infrastructure.Configurations;

namespace ExpenseService.Infrastructure.Persistence.Configurations;

public class ExpenseConfiguration : BaseEntityConfiguration<Expense>
{
    public override void Configure(EntityTypeBuilder<Expense> builder)
    {
        base.Configure(builder);

        builder.Property(e => e.Description)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(e => e.Amount)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(e => e.PaidByUserId)
            .IsRequired();

        builder.HasMany(e => e.Splits)
            .WithOne()
            .HasForeignKey("ExpenseId")
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class ExpenseSplitConfiguration : IEntityTypeConfiguration<ExpenseSplit>
{
    public void Configure(EntityTypeBuilder<ExpenseSplit> builder)
    {
        builder.HasKey(es => new { es.UserId, builder.Property<Guid>("ExpenseId").Metadata.Name });

        builder.Property(es => es.Amount)
            .IsRequired()
            .HasPrecision(18, 2);
    }
} 