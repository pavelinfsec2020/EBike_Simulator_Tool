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
            new Translation { Key = "welcome_message", RuString = "Добро пожаловать в симулятор!", EnString = "Welcome to the simulator!" },
            new Translation { Key = "start_button", RuString = "Старт", EnString = "Start" },
            new Translation { Key = "stop_button", RuString = "Стоп", EnString = "Stop" }
            };

            await context.Translations.AddRangeAsync(translations);
            await context.SaveChangesAsync();
        }
    }
}
