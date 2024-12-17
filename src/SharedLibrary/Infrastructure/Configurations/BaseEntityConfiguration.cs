using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SharedLibrary.Domain;

namespace SharedLibrary.Infrastructure.Configurations;

public abstract class BaseEntityConfiguration<T> : IEntityTypeConfiguration<T> where T : BaseEntity
{
    public virtual void Configure(EntityTypeBuilder<T> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.CreatedAt)
            .IsRequired();

        builder.Property(e => e.UpdatedAt);

        builder.Property(e => e.CreatedBy)
            .HasMaxLength(50);

        builder.Property(e => e.UpdatedBy)
            .HasMaxLength(50);

        builder.Property(e => e.IsDeleted)
            .IsRequired();

        builder.Property(e => e.IsActive)
            .IsRequired();

        // Add global query filter for soft delete
        builder.HasQueryFilter(e => !e.IsDeleted && e.IsActive);
    }
}