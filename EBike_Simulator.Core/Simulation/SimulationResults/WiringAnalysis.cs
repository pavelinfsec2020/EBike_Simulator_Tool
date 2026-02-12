using EBike_Simulator.Core.BikeComponents;
using System.Text;

namespace EBike_Simulator.Core.Simulation.SimulationResults
{
    /// <summary>
    /// Результаты анализа проводки
    /// </summary>
    public class WiringAnalysis
    {
        #region props

        /// <summary>Рекомендованные провода для каждого участка</summary>
        public Dictionary<string, Wire> RecommendedWires { get; set; } = new();

        /// <summary>Максимальный ток от батареи (А)</summary>
        public double MaxBatteryCurrent { get; set; }

        /// <summary>Максимальный ток к мотору (А)</summary>
        public double MaxMotorCurrent { get; set; }

        /// <summary>Средние потери мощности в проводке (Вт)</summary>
        public double EstimatedAvgPowerLoss { get; set; }

        #endregion

        #region public methods

        /// <summary>
        /// Вывести отчет по проводке в консоль
        /// </summary>
        public string PrintReport()
        {
            var result = new StringBuilder();

            result.AppendLine("\n=== АНАЛИЗ ПРОВОДКИ ===");
            result.AppendLine($"Пиковый ток (Батарея → Контроллер): {MaxBatteryCurrent:F1} А");
            result.AppendLine($"Пиковый ток (Контроллер → Мотор): {MaxMotorCurrent:F1} А");

            foreach (var kvp in RecommendedWires)
            {
                result.AppendLine($"\n{kvp.Key}:");
                result.AppendLine($"  Провод: ({kvp.Value.AWG} AWG)");
                result.AppendLine($"  Сечение: {kvp.Value.CrossSectionMm2:F2} мм²");
                result.AppendLine($"  Макс. ток: {kvp.Value.MaxCurrentAmp} А");
                result.AppendLine($"  Безопасность: {kvp.Value.GetSafetyStatus(kvp.Key.Contains("Battery") ? MaxBatteryCurrent : MaxMotorCurrent)}");
            }

            result.AppendLine($"\nСредние потери в проводке: {EstimatedAvgPowerLoss:F2} Вт");

            return result.ToString();
        }

        #endregion
    }
}
