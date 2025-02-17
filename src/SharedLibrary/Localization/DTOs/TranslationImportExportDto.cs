using System.Text.Json.Serialization;

namespace SharedLibrary.Localization.DTOs;

/// <summary>
/// DTO for importing and exporting translations
/// </summary>
public class TranslationImportExportDto
{
    [JsonPropertyName("key")]
    public string Key { get; set; } = string.Empty;

    [JsonPropertyName("culture")]
    public string Culture { get; set; } = string.Empty;

    [JsonPropertyName("value")]
    public string Value { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("group")]
    public string? Group { get; set; }

    [JsonPropertyName("isTemplate")]
    public bool IsTemplate { get; set; }
}

/// <summary>
/// Result of a translation import operation
/// </summary>
public class ImportResult
{
    public int Added { get; set; }
    public int Updated { get; set; }
    public int Skipped { get; set; }
    public List<string> Errors { get; set; } = [];
}

/// <summary>
/// Progress information for import operations
/// </summary>
public class ImportProgress
{
    /// <summary>
    /// Number of items processed so far
    /// </summary>
    public int ProcessedCount { get; set; }

    /// <summary>
    /// Total number of items to process
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// Number of items added
    /// </summary>
    public int Added { get; set; }

    /// <summary>
    /// Number of items updated
    /// </summary>
    public int Updated { get; set; }

    /// <summary>
    /// Number of items skipped
    /// </summary>
    public int Skipped { get; set; }

    /// <summary>
    /// Current progress percentage
    /// </summary>
    public double ProgressPercentage => TotalCount == 0 ? 0 : (double)ProcessedCount / TotalCount * 100;
}