using System.Text.RegularExpressions;
using SharedLibrary.Localization.DTOs;

namespace SharedLibrary.Localization.Validation;

/// <summary>
/// Provides validation rules for translation imports
/// </summary>
public static class TranslationValidationRules
{
    private static readonly Regex KeyFormatRegex = new(@"^[a-zA-Z0-9_\-.]+$", RegexOptions.Compiled);
    private static readonly Regex CultureFormatRegex = new(@"^[a-z]{2}(-[A-Z]{2})?$", RegexOptions.Compiled);
    private static readonly Regex TemplatePlaceholderRegex = new(@"\{[a-zA-Z0-9_]+\}", RegexOptions.Compiled);

    // Maximum lengths for various fields
    public const int MaxKeyLength = 100;
    public const int MaxValueLength = 4000;
    public const int MaxDescriptionLength = 500;
    public const int MaxGroupLength = 50;

    /// <summary>
    /// Validates a translation import DTO
    /// </summary>
    /// <param name="translation">The translation to validate</param>
    /// <param name="supportedCultures">List of supported cultures</param>
    /// <returns>A validation result indicating whether the translation is valid and any errors</returns>
    public static ValidationResult ValidateTranslation(TranslationImportExportDto translation, ISet<string> supportedCultures)
    {
        var errors = new List<string>();

        // Validate Key
        if (string.IsNullOrWhiteSpace(translation.Key))
        {
            errors.Add("Key is required");
        }
        else
        {
            if (translation.Key.Length > MaxKeyLength)
                errors.Add($"Key length exceeds maximum of {MaxKeyLength} characters");

            if (!KeyFormatRegex.IsMatch(translation.Key))
                errors.Add("Key contains invalid characters (allowed: letters, numbers, underscore, hyphen, dot)");
        }

        // Validate Culture
        if (string.IsNullOrWhiteSpace(translation.Culture))
        {
            errors.Add("Culture is required");
        }
        else
        {
            if (!CultureFormatRegex.IsMatch(translation.Culture))
                errors.Add("Invalid culture format (expected: 'xx' or 'xx-XX')");

            if (!supportedCultures.Contains(translation.Culture))
                errors.Add($"Culture '{translation.Culture}' is not in the list of supported cultures");
        }

        // Validate Value
        if (string.IsNullOrWhiteSpace(translation.Value))
        {
            errors.Add("Value is required");
        }
        else
        {
            if (translation.Value.Length > MaxValueLength)
                errors.Add($"Value length exceeds maximum of {MaxValueLength} characters");

            if (translation.IsTemplate && !ContainsPlaceholders(translation.Value))
                errors.Add("Template value must contain at least one placeholder (e.g., {name})");
        }

        // Validate Description (optional)
        if (!string.IsNullOrWhiteSpace(translation.Description) && translation.Description.Length > MaxDescriptionLength)
        {
            errors.Add($"Description length exceeds maximum of {MaxDescriptionLength} characters");
        }

        // Validate Group (optional)
        if (!string.IsNullOrWhiteSpace(translation.Group) && translation.Group.Length > MaxGroupLength)
        {
            errors.Add($"Group length exceeds maximum of {MaxGroupLength} characters");
        }

        return new ValidationResult(errors.Count == 0, errors);
    }

    private static bool ContainsPlaceholders(string value)
    {
        return TemplatePlaceholderRegex.IsMatch(value);
    }
}

/// <summary>
/// Represents the result of a validation operation
/// </summary>
public record ValidationResult(bool IsValid, IReadOnlyList<string> Errors);