using EBike_Simulator.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace EBike_Simulator.Data.Data
{
    public static class DbInitializer
    {
        public static async Task InitializeAsync(AppDbContext context)
        {
            await context.Database.MigrateAsync();

            if (await context.Translations.AnyAsync())          
                return;
            
            var translations = new[]
            {
            new Translation { Key = "criticalOverheating", RuString = "Критический перегрев", EnString = "Critical overheating" },
            new Translation { Key = "overheating", RuString = "Перегрев", EnString = "Overheating" },
            new Translation { Key = "warm", RuString = "Тепло", EnString = "Warm" },
            new Translation { Key = "standard", RuString = "Норма", EnString = "Standard" },
            };

            await context.Translations.AddRangeAsync(translations);
            await context.SaveChangesAsync();
        }
    }
}
