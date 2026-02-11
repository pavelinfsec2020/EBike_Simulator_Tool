namespace EBike_Simulator.Core.Models.Simulation
{
    /// <summary>
    /// Сводная статистика симуляции
    /// </summary>
    public class SimulationSummary
    {
        #region props

        /// <summary>Общая дистанция (км)</summary>
        public double TotalDistance { get; set; }

        /// <summary>Общее время (секунды)</summary>
        public double TotalTime { get; set; }

        /// <summary>Максимальная скорость (км/ч)</summary>
        public double MaxSpeed { get; set; }

        /// <summary>Средняя скорость (км/ч)</summary>
        public double AverageSpeed { get; set; }

        /// <summary>Средняя мощность (Вт)</summary>
        public double AveragePower { get; set; }

        /// <summary>Пиковая мощность (Вт)</summary>
        public double PeakPower { get; set; }

        /// <summary>Потребленная энергия (Вт·ч)</summary>
        public double EnergyConsumed { get; set; }

        /// <summary>Энергоэффективность (км/кВт·ч)</summary>
        public double EnergyEfficiency { get; set; }

        /// <summary>Максимальная температура мотора (°C)</summary>
        public double MaxMotorTemp { get; set; }

        /// <summary>Максимальная температура контроллера (°C)</summary>
        public double MaxControllerTemp { get; set; }

        /// <summary>Конечная температура батареи (°C)</summary>
        public double FinalBatteryTemp { get; set; }

        /// <summary>Флаг разряда батареи</summary>
        public bool BatteryDepleted { get; set; }

        /// <summary>Среднее влияние ветра (%)</summary>
        public double WindImpact { get; set; }

        /// <summary>Среднее влияние температуры (%)</summary>
        public double TempImpact { get; set; }

        /// <summary>Время в нормальном тепловом режиме (сек)</summary>
        public double NormalOperatingTime { get; set; }

        /// <summary>Время в режиме предупреждения (сек)</summary>
        public double WarningOperatingTime { get; set; }

        /// <summary>Время в критическом режиме (сек)</summary>
        public double CriticalOperatingTime { get; set; }

        #endregion

        #region public methods

        /// <summary>
        /// Получить процентное соотношение тепловых режимов
        /// </summary>
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

        /// <summary>
        /// Получить текстовый отчет
        /// </summary>
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

        #endregion
    }
}
