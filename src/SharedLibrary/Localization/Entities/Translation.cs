using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SharedLibrary.Localization.Entities;

/// <summary>
/// Represents a translation entry in the database
/// </summary>
public class Translation
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(255)]
    public string Key { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string Culture { get; set; } = string.Empty;

    [Required]
    public string Value { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    public bool IsTemplate { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    [MaxLength(255)]
    public string? Group { get; set; }

    [NotMapped]
    public bool IsMissing { get; set; }
}