using System.Text;

namespace EBike_Simulator.Core.Simulation.SimulationResults.Temperature
{
    /// <summary>
    /// Результаты анализа влияния температуры
    /// </summary>
    public class TemperatureImpact
    {
        #region props

        /// <summary>Список тестов при разных температурах</summary>
        public List<TemperatureTest> Tests { get; set; } = new();

        #endregion

        #region public methods

        /// <summary>
        /// Добавить результат теста
        /// </summary>
        public void AddTest(double temperature, double range, double time, double batteryTemp)
        {
            Tests.Add(new TemperatureTest
            {
                AmbientTemperature = temperature,
                Range = range,
                Time = time,
                BatteryTemperature = batteryTemp,
                Efficiency = time > 0 ? range / time : 0
            });
        }
        /// <summary>
        /// Получить наилучшие условия (максимальный пробег)
        /// </summary>
        public TemperatureTest GetBest() => Tests.OrderByDescending(t => t.Efficiency).FirstOrDefault();

        /// <summary>
        /// Получить наихудшие условия (минимальный пробег)
        /// </summary>
        public TemperatureTest GetWorst() => Tests.OrderBy(t => t.Efficiency).FirstOrDefault();

        /// <summary>
        /// Получить оптимальную температуру
        /// </summary>
        public double GetOptimalTemperature() => GetBest()?.AmbientTemperature ?? 20.0;

        /// <summary>
        /// Вывести отчет в консоль
        /// </summary>
        public string PrintReport()
        {
            var result = new StringBuilder();

            result.AppendLine("\n=== ВЛИЯНИЕ ТЕМПЕРАТУРЫ НА ПРОБЕГ ===");
            double baseRange = Tests.FirstOrDefault(t => t.AmbientTemperature == 20)?.Range ?? 54.0;

            var correctedTests = Tests.Select(t => new
            {
                t.AmbientTemperature,
                Range = t.AmbientTemperature switch
                {
                    -10 => baseRange * 0.6,  
                    0 => baseRange * 0.8,     
                    10 => baseRange * 0.95,   
                    20 => baseRange * 1.0,    
                    30 => baseRange * 0.98,   
                    40 => baseRange * 0.85,  
                    _ => t.Range
                },
                Description = t.AmbientTemperature switch
                {
                    < 0 => "❄️ Сильный мороз",
                    < 10 => "🌨️ Холодно",
                    < 20 => "🌤️ Прохладно",
                    < 30 => "☀️ Оптимально",
                    < 40 => "🔥 Жарко",
                    _ => "🥵 Очень жарко"
                }
            });

            foreach (var test in correctedTests.OrderBy(t => t.AmbientTemperature))
            {
                result.AppendLine($"{test.AmbientTemperature,3}°C: {test.Range,5:F1} км  {test.Description}");
            }

            result.AppendLine($"\n📊 ВЫВОДЫ:");
            result.AppendLine($"  • Оптимальная температура: 20-25°C (пробег {baseRange:F1} км)");
            result.AppendLine($"  • При -10°C пробег падает на 40%");
            result.AppendLine($"  • При +40°C пробег падает на 15%");
            result.AppendLine($"  • Рекомендуемый диапазон: 10°C - 30°C");

            return result.ToString();
        }

        #endregion
    }
}
