using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using TranslationService.Models;

namespace TranslationService.Data;

public class TranslationDbContext : DbContext
{
    public DbSet<Translation> Translations { get; set; } = null!;

    public TranslationDbContext(DbContextOptions<TranslationDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Translation>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.SourceText, e.SourceLanguage, e.TargetLanguage })
                .IsUnique();

            entity.Property(e => e.SourceText).IsRequired().HasMaxLength(4000);
            entity.Property(e => e.TranslatedText).IsRequired().HasMaxLength(4000);
            entity.Property(e => e.SourceLanguage).IsRequired().HasMaxLength(10);
            entity.Property(e => e.TargetLanguage).IsRequired().HasMaxLength(10);
            entity.Property(e => e.CreatedAt)
                .HasColumnType("datetime2")
                .ValueGeneratedOnAdd();
        });
    }
}