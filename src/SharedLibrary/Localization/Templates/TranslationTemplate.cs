using System.Text.RegularExpressions;

namespace SharedLibrary.Localization.Templates;

/// <summary>
/// Represents a template for dynamic translations
/// </summary>
public class TranslationTemplate
{
    private static readonly Regex PlaceholderRegex = new(@"\{([^{}]+)\}", RegexOptions.Compiled);

    /// <summary>
    /// The template text with placeholders
    /// </summary>
    public string Template { get; }

    /// <summary>
    /// Dictionary of formatters for each placeholder
    /// </summary>
    public IReadOnlyDictionary<string, Func<object, string>> Formatters { get; }

    public TranslationTemplate(string template, Dictionary<string, Func<object, string>>? formatters = null)
    {
        Template = template;
        Formatters = formatters ?? new Dictionary<string, Func<object, string>>();
        ValidateTemplate();
    }

    /// <summary>
    /// Formats the template with the provided values
    /// </summary>
    public string Format(IDictionary<string, object> values)
    {
        return PlaceholderRegex.Replace(Template, match =>
        {
            var key = match.Groups[1].Value;
            if (!values.TryGetValue(key, out var value))
                return match.Value; // Keep original placeholder if value not provided

            if (Formatters.TryGetValue(key, out var formatter))
                return formatter(value);

            return value.ToString() ?? match.Value;
        });
    }

    private void ValidateTemplate()
    {
        var placeholders = PlaceholderRegex.Matches(Template)
            .Select(m => m.Groups[1].Value)
            .ToHashSet();

        // Ensure all formatters have corresponding placeholders
        var invalidFormatters = Formatters.Keys
            .Where(key => !placeholders.Contains(key))
            .ToList();

        if (invalidFormatters.Any())
        {
            throw new ArgumentException(
                $"Formatters found without corresponding placeholders: {string.Join(", ", invalidFormatters)}");
        }
    }
}