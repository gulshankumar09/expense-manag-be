using AuthService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuthService.Infrastructure.Persistence.Configurations;

public class RoleSettingsConfiguration : IEntityTypeConfiguration<RoleSettings>
{
    public void Configure(EntityTypeBuilder<RoleSettings> builder)
    {
        builder.ToTable("RoleSettings");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.MaxSuperAdminUsers)
            .IsRequired()
            .HasDefaultValue(1);

        // Ensure only one settings record exists
        builder.HasData(new RoleSettings
        {
            Id = 1,
            MaxSuperAdminUsers = 1
        });
    }
}