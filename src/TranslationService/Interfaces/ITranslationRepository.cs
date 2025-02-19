using TranslationService.Models;

namespace TranslationService.Interfaces;

/// <summary>
/// Repository interface for managing translations
/// </summary>
public interface ITranslationRepository
{
    /// <summary>
    /// Finds an existing translation
    /// </summary>
    Task<Translation?> FindTranslationAsync(
        string sourceText,
        string sourceLanguage,
        string targetLanguage);

    /// <summary>
    /// Saves a new translation
    /// </summary>
    Task<Translation> SaveTranslationAsync(Translation translation);

    /// <summary>
    /// Updates the usage statistics of a translation
    /// </summary>
    Task UpdateTranslationUsageAsync(int translationId);
}