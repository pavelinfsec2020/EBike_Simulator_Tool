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
                Efficiency = range / Math.Max(time, 0.1)
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

            foreach (var test in Tests.OrderBy(t => t.AmbientTemperature))
            {
                result.AppendLine($"{test.AmbientTemperature,3}°C: {test.Range,5:F1} км (эфф.: {test.Efficiency,4:F1} км/ч)");
            }

            var best = GetBest();
            var worst = GetWorst();

            if (best != null && worst != null)
            {
                result.AppendLine($"\nОптимальная температура: {best.AmbientTemperature}°C");
                result.AppendLine($"Разница пробега: {best.Range - worst.Range:F1} км ({((best.Range - worst.Range) / worst.Range * 100):F1}%)");
            }

            return result.ToString();
            
        }

        #endregion
    }
}
