using EBike_Simulator.Core.BikeComponents;

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
            string output =
    "\n=== АНАЛИЗ ПРОВОДКИ ===" +
    $"\nПиковый ток (Батарея → Контроллер): {MaxBatteryCurrent:F1} А" +
    $"\nПиковый ток (Контроллер → Мотор): {MaxMotorCurrent:F1} А" +
   
    string.Join("", RecommendedWires.Select(kvp =>
        $"\n{kvp.Key}:" +
        $"\n  Провод: ({kvp.Value.AWG} AWG)" +
        $"\n  Сечение: {kvp.Value.CrossSectionMm2:F2} мм²" +
        $"\n  Макс. ток: {kvp.Value.MaxCurrentAmp} А" +
        $"\n  Безопасность: {kvp.Value.GetSafetyStatus(kvp.Key.Contains("Battery") ? MaxBatteryCurrent : MaxMotorCurrent)}"
    )) +
    $"\n\nСредние потери в проводке: {EstimatedAvgPowerLoss:F2} Вт";

            return output ;
        }

        #endregion
    }
}
