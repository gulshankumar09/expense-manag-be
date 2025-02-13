using System.ComponentModel.DataAnnotations;
using Ganss.Xss;

namespace SharedLibrary.Validation;

/// <summary>
/// Validates and sanitizes input to prevent XSS attacks
/// </summary>
/// <remarks>
/// Initializes a new instance of the NoXssAttribute
/// </remarks>
/// <param name="sanitize">If true, the input will be sanitized. If false, only validation will be performed.</param>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
public class NoXssAttribute(bool sanitize = true) : ValidationAttribute
{
    private static readonly HtmlSanitizer Sanitizer = new();
    private readonly bool _sanitize = sanitize;

    static NoXssAttribute()
    {
        // Configure the sanitizer with safe settings
        Sanitizer.AllowedTags.Clear();
        Sanitizer.AllowedAttributes.Clear();
        Sanitizer.AllowedCssProperties.Clear();
        Sanitizer.AllowedSchemes.Clear();

        // Add allowed HTML tags
        Sanitizer.AllowedTags.Add("b");
        Sanitizer.AllowedTags.Add("i");
        Sanitizer.AllowedTags.Add("u");
        Sanitizer.AllowedTags.Add("strong");
        Sanitizer.AllowedTags.Add("em");
        Sanitizer.AllowedTags.Add("br");
        Sanitizer.AllowedTags.Add("p");

        // Add allowed attributes
        Sanitizer.AllowedAttributes.Add("title");
        Sanitizer.AllowedAttributes.Add("href");

        // Add allowed URL schemes
        Sanitizer.AllowedSchemes.Add("http");
        Sanitizer.AllowedSchemes.Add("https");
        Sanitizer.AllowedSchemes.Add("mailto");
        Sanitizer.AllowedSchemes.Add("tel");
    }

    /// <summary>
    /// Validates the input value for potential XSS content
    /// </summary>
    /// <param name="value">The value to validate</param>
    /// <param name="validationContext">The validation context</param>
    /// <returns>ValidationResult indicating whether the value is valid</returns>
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value == null)
            return ValidationResult.Success;

        var stringValue = value.ToString();
        if (string.IsNullOrWhiteSpace(stringValue))
            return ValidationResult.Success;

        // Check if the input contains potential XSS content
        var sanitized = Sanitizer.Sanitize(stringValue);
        if (sanitized != stringValue)
        {
            if (_sanitize)
            {
                // If sanitization is enabled, modify the property value
                var property = validationContext.ObjectType.GetProperty(validationContext.MemberName!);
                if (property != null && property.CanWrite)
                {
                    property.SetValue(validationContext.ObjectInstance, sanitized);
                    return ValidationResult.Success;
                }
            }

            return new ValidationResult(
                $"The field {validationContext.DisplayName} contains potentially dangerous content.",
                new[] { validationContext.MemberName! });
        }

        return ValidationResult.Success;
    }
}