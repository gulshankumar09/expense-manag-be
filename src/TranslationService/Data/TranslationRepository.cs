using Microsoft.EntityFrameworkCore;
using TranslationService.Interfaces;
using TranslationService.Models;

namespace TranslationService.Data;

public class TranslationRepository : ITranslationRepository
{
    private readonly TranslationDbContext _context;

    public TranslationRepository(TranslationDbContext context)
    {
        _context = context;
    }

    public async Task<Translation?> FindTranslationAsync(
        string sourceText,
        string sourceLanguage,
        string targetLanguage)
    {
        return await _context.Translations
            .FirstOrDefaultAsync(t =>
                t.SourceText == sourceText &&
                t.SourceLanguage == sourceLanguage &&
                t.TargetLanguage == targetLanguage);
    }

    public async Task<Translation> SaveTranslationAsync(Translation translation)
    {
        _context.Translations.Add(translation);
        await _context.SaveChangesAsync();
        return translation;
    }

    public async Task UpdateTranslationUsageAsync(int translationId)
    {
        await _context.Translations
            .Where(t => t.Id == translationId)
            .ExecuteUpdateAsync(s => s
                .SetProperty(t => t.LastUsedAt, DateTime.UtcNow)
                .SetProperty(t => t.UseCount, t => t.UseCount + 1));
    }
}