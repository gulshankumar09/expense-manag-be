using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SharedLibrary.Configuration;
using SharedLibrary.Localization.DTOs;
using SharedLibrary.Localization.Entities;
using SharedLibrary.Localization.Validation;

namespace SharedLibrary.Localization.Services;

/// <summary>
/// Service for handling bulk import and export of translations
/// </summary>
public class TranslationImportExportService
{
    private readonly DbContext _dbContext;
    private readonly ILogger<TranslationImportExportService> _logger;
    private readonly HashSet<string> _supportedCultures;
    private const int BatchSize = 1000;

    public TranslationImportExportService(
        DbContext dbContext,
        ILogger<TranslationImportExportService> logger,
        LocalizationSettings settings)
    {
        _dbContext = dbContext;
        _logger = logger;
        _supportedCultures = new HashSet<string>(settings.SupportedCultures);
    }

    /// <summary>
    /// Exports translations to JSON format
    /// </summary>
    public async Task<string> ExportToJsonAsync(string? culture = null, string? group = null)
    {
        var query = _dbContext.Set<Translation>().AsQueryable();

        if (!string.IsNullOrEmpty(culture))
            query = query.Where(t => t.Culture == culture);

        if (!string.IsNullOrEmpty(group))
            query = query.Where(t => t.Group == group);

        var translations = await query
            .Select(t => new TranslationImportExportDto
            {
                Key = t.Key,
                Culture = t.Culture,
                Value = t.Value,
                Description = t.Description,
                Group = t.Group,
                IsTemplate = t.IsTemplate
            })
            .ToListAsync();

        return JsonSerializer.Serialize(translations, new JsonSerializerOptions
        {
            WriteIndented = true
        });
    }

    /// <summary>
    /// Validates a collection of translations
    /// </summary>
    private (bool IsValid, List<ValidationError> Errors) ValidateImport(IEnumerable<TranslationImportExportDto> translations)
    {
        var errors = new List<ValidationError>();
        var processedKeys = new HashSet<(string Key, string Culture)>();

        foreach (var translation in translations)
        {
            var keyCulturePair = (translation.Key, translation.Culture);

            // Check for duplicates in the import
            if (!processedKeys.Add(keyCulturePair))
            {
                errors.Add(new ValidationError(
                    translation.Key,
                    translation.Culture,
                    "Duplicate key-culture combination in import"));
                continue;
            }

            // Validate individual translation
            var validationResult = TranslationValidationRules.ValidateTranslation(translation, _supportedCultures);
            if (!validationResult.IsValid)
            {
                errors.Add(new ValidationError(
                    translation.Key,
                    translation.Culture,
                    string.Join("; ", validationResult.Errors)));
            }
        }

        return (!errors.Any(), errors);
    }

    private async Task<ImportResult> ProcessImportAsync(
        IEnumerable<TranslationImportExportDto> translations,
        bool overwrite,
        IProgress<ImportProgress>? progress)
    {
        var result = new ImportResult();

        // Validate all translations first
        var (isValid, validationErrors) = ValidateImport(translations);
        if (!isValid)
        {
            result.Errors.AddRange(validationErrors.Select(e =>
                $"Validation error for {e.Key} ({e.Culture}): {e.Message}"));
            return result;
        }

        var translationsList = translations.ToList();
        var totalCount = translationsList.Count;
        var processedCount = 0;

        // Process in batches
        foreach (var batch in translationsList.Chunk(BatchSize))
        {
            using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                foreach (var translation in batch)
                {
                    await ProcessTranslationImportAsync(translation, overwrite, result);
                    processedCount++;

                    progress?.Report(new ImportProgress
                    {
                        ProcessedCount = processedCount,
                        TotalCount = totalCount,
                        Added = result.Added,
                        Updated = result.Updated,
                        Skipped = result.Skipped
                    });
                }

                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error processing batch. Rolling back.");
                result.Errors.Add($"Batch processing failed: {ex.Message}");
            }
        }

        return result;
    }

    /// <summary>
    /// Imports translations from JSON format with validation and batch processing
    /// </summary>
    public async Task<ImportResult> ImportFromJsonAsync(string json, bool overwrite = false,
        IProgress<ImportProgress>? progress = null)
    {
        try
        {
            var importedTranslations = JsonSerializer.Deserialize<List<TranslationImportExportDto>>(json);
            if (importedTranslations == null)
            {
                return new ImportResult { Errors = { "Invalid JSON format" } };
            }

            return await ProcessImportAsync(importedTranslations, overwrite, progress);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during JSON import");
            return new ImportResult { Errors = { $"Import failed: {ex.Message}" } };
        }
    }

    /// <summary>
    /// Imports translations from CSV format with validation and batch processing
    /// </summary>
    public async Task<ImportResult> ImportFromCsvAsync(string csv, bool overwrite = false,
        IProgress<ImportProgress>? progress = null)
    {
        var translations = new List<TranslationImportExportDto>();

        try
        {
            using var reader = new StringReader(csv);
            // Skip header
            reader.ReadLine();

            string? line;
            while ((line = reader.ReadLine()) != null)
            {
                try
                {
                    var fields = ParseCsvLine(line);
                    if (fields.Count < 6) continue;

                    translations.Add(new TranslationImportExportDto
                    {
                        Key = fields[0],
                        Culture = fields[1],
                        Value = fields[2],
                        Description = fields[3],
                        Group = fields[4],
                        IsTemplate = bool.Parse(fields[5])
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error parsing CSV line: {Line}", line);
                    return new ImportResult { Errors = { $"Error parsing line: {line}. Error: {ex.Message}" } };
                }
            }

            return await ProcessImportAsync(translations, overwrite, progress);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during CSV import");
            return new ImportResult { Errors = { $"Import failed: {ex.Message}" } };
        }
    }

    private async Task ProcessTranslationImportAsync(TranslationImportExportDto importedTranslation,
        bool overwrite, ImportResult result)
    {
        try
        {
            var existing = await _dbContext.Set<Translation>()
                .FirstOrDefaultAsync(t =>
                    t.Key == importedTranslation.Key &&
                    t.Culture == importedTranslation.Culture);

            if (existing != null)
            {
                if (!overwrite)
                {
                    result.Skipped++;
                    return;
                }

                existing.Value = importedTranslation.Value;
                existing.Description = importedTranslation.Description;
                existing.Group = importedTranslation.Group;
                existing.IsTemplate = importedTranslation.IsTemplate;
                existing.UpdatedAt = DateTime.UtcNow;
                result.Updated++;
            }
            else
            {
                var newTranslation = new Translation
                {
                    Key = importedTranslation.Key,
                    Culture = importedTranslation.Culture,
                    Value = importedTranslation.Value,
                    Description = importedTranslation.Description,
                    Group = importedTranslation.Group,
                    IsTemplate = importedTranslation.IsTemplate
                };

                _dbContext.Set<Translation>().Add(newTranslation);
                result.Added++;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing translation: {Key} - {Culture}",
                importedTranslation.Key, importedTranslation.Culture);
            result.Errors.Add($"Error importing {importedTranslation.Key} ({importedTranslation.Culture}): {ex.Message}");
        }
    }

    /// <summary>
    /// Exports translations to CSV format
    /// </summary>
    public async Task<string> ExportToCsvAsync(string? culture = null, string? group = null)
    {
        var query = _dbContext.Set<Translation>().AsQueryable();

        if (!string.IsNullOrEmpty(culture))
            query = query.Where(t => t.Culture == culture);

        if (!string.IsNullOrEmpty(group))
            query = query.Where(t => t.Group == group);

        var translations = await query.ToListAsync();

        using var writer = new StringWriter();
        writer.WriteLine("Key,Culture,Value,Description,Group,IsTemplate");

        foreach (var translation in translations)
        {
            writer.WriteLine($"\"{EscapeCsvField(translation.Key)}\",\"{EscapeCsvField(translation.Culture)}\",\"{EscapeCsvField(translation.Value)}\",\"{EscapeCsvField(translation.Description)}\",\"{EscapeCsvField(translation.Group)}\",{translation.IsTemplate}");
        }

        return writer.ToString();
    }

    private static string EscapeCsvField(string? field)
    {
        if (string.IsNullOrEmpty(field)) return string.Empty;
        return field.Replace("\"", "\"\"");
    }

    private static List<string> ParseCsvLine(string line)
    {
        var fields = new List<string>();
        var currentField = new System.Text.StringBuilder();
        var inQuotes = false;

        for (int i = 0; i < line.Length; i++)
        {
            if (line[i] == '\"')
            {
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '\"')
                {
                    currentField.Append('\"');
                    i++;
                }
                else
                {
                    inQuotes = !inQuotes;
                }
            }
            else if (line[i] == ',' && !inQuotes)
            {
                fields.Add(currentField.ToString());
                currentField.Clear();
            }
            else
            {
                currentField.Append(line[i]);
            }
        }

        fields.Add(currentField.ToString());
        return fields;
    }
}

/// <summary>
/// Represents a validation error for a specific translation
/// </summary>
public record ValidationError(string Key, string Culture, string Message);