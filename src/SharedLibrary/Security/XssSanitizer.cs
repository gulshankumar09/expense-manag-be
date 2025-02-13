using Ganss.Xss;

namespace SharedLibrary.Security;

/// <summary>
/// Provides methods to sanitize strings and prevent XSS attacks
/// </summary>
public static class XssSanitizer
{
    private static readonly HtmlSanitizer Sanitizer;

    static XssSanitizer()
    {
        Sanitizer = new HtmlSanitizer();

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
    /// Sanitizes the input string by removing potentially dangerous content
    /// </summary>
    /// <param name="input">The input string to sanitize</param>
    /// <returns>The sanitized string</returns>
    public static string Sanitize(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        return Sanitizer.Sanitize(input);
    }

    /// <summary>
    /// Checks if the input string contains potentially dangerous content
    /// </summary>
    /// <param name="input">The input string to check</param>
    /// <returns>True if the string is safe, false if it contains potentially dangerous content</returns>
    public static bool IsSafe(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return true;

        return Sanitizer.Sanitize(input) == input;
    }

    /// <summary>
    /// Removes all HTML tags from the input string
    /// </summary>
    /// <param name="input">The input string to clean</param>
    /// <returns>The string with all HTML tags removed</returns>
    public static string StripHtml(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        return Sanitizer.Sanitize(input);
    }
}