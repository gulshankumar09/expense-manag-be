using AuthService.Domain.Entities;
using AuthService.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SharedLibrary.Infrastructure.Configurations;

namespace AuthService.Infrastructure.Persistence.Configurations;

public class UserConfiguration : BaseEntityConfiguration<User>
{
    public override void Configure(EntityTypeBuilder<User> builder)
    {
        base.Configure(builder);

        builder.Property(u => u.Email)
            .HasConversion(
                email => email.Value,
                value => Email.Create(value))
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(u => u.PasswordHash)
            .HasConversion(
                password => password.Hash,
                value => Password.Create(value))
            .IsRequired();

        builder.HasIndex(u => u.Email)
            .IsUnique();

        builder.OwnsOne(u => u.Name, nb =>
        {
            nb.Property(n => n.FirstName)
            .IsRequired()
            .HasMaxLength(50)
            .HasColumnName("FirstName");

            nb.Property(n => n.LastName)
            .IsRequired()
            .HasMaxLength(50)
            .HasColumnName("LastName");
        });

        builder.Property(u => u.GoogleId)
            .HasMaxLength(100);

        builder.Property(u => u.PhoneNumber)
            .HasMaxLength(20);

        builder.Property(u => u.VerificationToken)
            .HasMaxLength(100);

        builder.Property(u => u.RefreshToken)
            .HasMaxLength(100);
    }
} 