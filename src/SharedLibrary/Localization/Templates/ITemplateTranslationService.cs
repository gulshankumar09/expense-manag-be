namespace SharedLibrary.Localization.Templates;

/// <summary>
/// Interface for template-based translation service
/// </summary>
public interface ITemplateTranslationService
{
    /// <summary>
    /// Gets a formatted translation using a template and dynamic values
    /// </summary>
    /// <param name="key">The translation key</param>
    /// <param name="culture">The target culture</param>
    /// <param name="values">Dictionary of values to replace in the template</param>
    /// <returns>Formatted translation string</returns>
    string GetTemplatedTranslation(string key, string culture, IDictionary<string, object> values);

    /// <summary>
    /// Gets a user-specific translation with formatted values
    /// </summary>
    /// <param name="key">The translation key</param>
    /// <param name="userId">The user's ID</param>
    /// <param name="culture">The target culture</param>
    /// <param name="values">Dictionary of values to replace in the template</param>
    /// <returns>Formatted translation string with user-specific values</returns>
    string GetUserTranslation(string key, string userId, string culture, IDictionary<string, object> values);
}