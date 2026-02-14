using EBike_Simulator.Core.Enums;
using System.Text;

namespace EBike_Simulator.Core.Simulation.SimulationResults.Wind
{
    /// <summary>
    /// Результаты анализа влияния ветра
    /// </summary>
    public class WindImpactTest
    {
        #region props

        /// <summary>Список результатов тестов</summary>
        public List<WindImpactResult> Results { get; set; } = new();

        #endregion

        #region public methods

        /// <summary>
        /// Добавить результат теста
        /// </summary>
        public void AddResult(WindDirection direction, double windSpeed, double range, double time)
        {
            Results.Add(new WindImpactResult
            {
                Direction = direction,
                WindSpeed = windSpeed,
                Range = range,
                Time = time,
                Efficiency = range / Math.Max(time, 0.1)
            });
        }

        /// <summary>
        /// Получить наилучшие условия
        /// </summary>
        public WindImpactResult GetBestResult() => Results.OrderByDescending(r => r.Efficiency).FirstOrDefault();

        /// <summary>
        /// Получить наихудшие условия
        /// </summary>
        public WindImpactResult GetWorstResult() => Results.OrderBy(r => r.Efficiency).FirstOrDefault();

        public string PrintReport()
        {
            var result = new StringBuilder();
            result.AppendLine("\n=== ВЛИЯНИЕ ВЕТРА НА ПРОБЕГ ===");

            var grouped = Results.GroupBy(r => r.Direction);

            foreach (var group in grouped)
            {
                result.AppendLine($"\n{GetDirectionName(group.Key)}:");

                foreach (var item in group.OrderBy(r => r.WindSpeed))
                {
                    double range = Math.Min(item.Range, 150);
                    result.AppendLine($"  {item.WindSpeed,3} м/с: {range,5:F1} км");
                }
            }

            var best = GetBestResult();
            var worst = GetWorstResult();

            if (best != null && worst != null)
            {
                double gain = Math.Min(((best.Range / worst.Range - 1) * 100), 200);

                result.AppendLine($"\n📊 ВЫВОДЫ:");
                result.AppendLine($"  • Лучшие условия: {best.WindSpeed} м/с ({GetDirectionName(best.Direction)})");
                result.AppendLine($"  • Худшие условия: {worst.WindSpeed} м/с ({GetDirectionName(worst.Direction)})");
                result.AppendLine($"  • Выигрыш от попутного ветра: +{gain:F1}%");
            }

            return result.ToString();
        }
        #endregion

        #region private methods

        private string GetDirectionName(WindDirection direction) => direction switch
        {
            WindDirection.Headwind => "Встречный ветер",
            WindDirection.Tailwind => "Попутный ветер",
            WindDirection.Crosswind => "Боковой ветер",
            _ => "Нет ветра"
        };

        #endregion
    }
}
