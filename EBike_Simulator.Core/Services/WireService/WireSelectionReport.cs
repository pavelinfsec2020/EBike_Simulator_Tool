using System;
using System.Collections.Generic;
using System.Text;

namespace EBike_Simulator.Core.Services.WireService
{
    /// <summary>
    /// Отчет по анализу проводки
    /// </summary>
    public class WireSelectionReport
    {
        #region props

        /// <summary>Результаты анализа для каждой длины</summary>
        public List<WireAnalysis> Wires { get; set; } = new();

        #endregion

        #region public methods

        /// <summary>
        /// Вывести отчет в консоль
        /// </summary>
        public string PrintReport()
        {
            var result = new StringBuilder();

            result.AppendLine("\n=== ОТЧЕТ ПО ПРОВОДКЕ ===");

            foreach (var analysis in Wires)
            {
                result.AppendLine($"\nДлина: {analysis.Length:F2} м");
                result.AppendLine($"Провод: ({analysis.SelectedWire.AWG} AWG)");
                result.AppendLine($"Падение напряжения: {analysis.VoltageDrop:F3} В ({analysis.VoltageDropPercent:F2}%)");
                result.AppendLine($"Потери мощности: {analysis.PowerLoss:F1} Вт");
                result.AppendLine($"Эффективность: {analysis.Efficiency:P1}");
                result.AppendLine($"Безопасность: {analysis.SafetyStatus}");
            }

            double totalLoss = Wires.Sum(w => w.PowerLoss);
            double avgEfficiency = Wires.Average(w => w.Efficiency);

            result.AppendLine($"\nИтого:");
            result.AppendLine($"• Суммарные потери: {totalLoss:F1} Вт");
            result.AppendLine($"• Средняя эффективность: {avgEfficiency:P1}");

            return result.ToString();
        }

        #endregion
    }
}
