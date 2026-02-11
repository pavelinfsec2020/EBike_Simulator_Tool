using System;
using System.Collections.Generic;
using System.Text;

namespace EBike_Simulator.Core.Models.Simulation
{
    public class SimulationSummary
    {
        #region props

        public double TotalDistance { get; set; }
        public double TotalTime { get; set; }
        public double MaxSpeed { get; set; }
        public double AverageSpeed { get; set; }
        public double AveragePower { get; set; }
        public double PeakPower { get; set; }
        public double EnergyConsumed { get; set; }
        public double EnergyEfficiency { get; set; }
        public double MaxMotorTemp { get; set; }
        public double MaxControllerTemp { get; set; }
        public double FinalBatteryTemp { get; set; }
        public bool BatteryDepleted { get; set; }
        public double WindImpact { get; set; }
        public double TempImpact { get; set; }
        public double NormalOperatingTime { get; set; }
        public double WarningOperatingTime { get; set; }
        public double CriticalOperatingTime { get; set; }

        #endregion

        #region methods

        public (double normalPercent, double warningPercent, double criticalPercent)
            GetThermalTimePercentages()
        {
            double total = NormalOperatingTime + WarningOperatingTime + CriticalOperatingTime;
            if (total <= 0) return (0, 0, 0);

            return (
                NormalOperatingTime / total * 100,
                WarningOperatingTime / total * 100,
                CriticalOperatingTime / total * 100
            );
        }

        public string GetReport()
        {
            var thermal = GetThermalTimePercentages();
            return $@"=== СВОДНЫЙ ОТЧЕТ ===

Дистанция и время:
• Пройдено: {TotalDistance:F1} км
• Время: {TotalTime / 60:F1} мин
• Макс. скорость: {MaxSpeed:F1} км/ч
• Ср. скорость: {AverageSpeed:F1} км/ч

Энергетика:
• Ср. мощность: {AveragePower:F0} Вт
• Пик мощность: {PeakPower:F0} Вт
• Энергия: {EnergyConsumed:F0} Вт·ч
• Эффективность: {EnergyEfficiency:F1} км/кВт·ч

Температура:
• Мотор макс.: {MaxMotorTemp:F1}°C
• Контроллер макс.: {MaxControllerTemp:F1}°C
• Батарея: {FinalBatteryTemp:F1}°C
• Батарея: {(BatteryDepleted ? "Разряжена" : "Есть заряд")}

Тепловые режимы:
• Нормальный: {thermal.normalPercent:F1}%
• Предупреждение: {thermal.warningPercent:F1}%
• Критический: {thermal.criticalPercent:F1}%

Внешние факторы:
• Ветер: {WindImpact:F1}%
• Температура: {TempImpact:F1}%";
        }
    }

    #endregion
}
