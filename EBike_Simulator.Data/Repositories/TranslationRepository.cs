using EBike_Simulator.Core.Enums;
using EBike_Simulator.Core.Interfaces.Repositories;
using EBike_Simulator.Data.Data;
using Microsoft.EntityFrameworkCore;

namespace EBike_Simulator.Data.Repositories
{
    /// <summary>
    /// Реализация репозитория для работы с переводами через EF Core
    /// </summary>
    public class TranslationRepository : ITranslationRepository
    {
        private readonly AppDbContext _context;

        public TranslationRepository(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Получить перевод по ключу и языку
        /// </summary>
        public async Task<string?> GetTranslationAsync(string key, Language language)
        {
            var translation = await _context.Translations
                .AsNoTracking()  
                .FirstOrDefaultAsync(t => t.Key == key);

            if (translation == null)
                return null;

            return language switch
            {
                Language.Ru => translation.RuString,
                Language.En => translation.EnString,
                _ => null
            };
        }
    }
}
