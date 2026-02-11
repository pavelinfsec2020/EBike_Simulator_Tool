namespace EBike_Simulator.Core.BikeComponents
{
    /// <summary>
    /// Модель электрического провода с характеристиками AWG
    /// </summary>
    public class Wire
    {
        #region props
      
        /// <summary>
        /// Калибр провода по стандарту AWG (чем меньше число, тем толще провод)
        /// </summary>
        public int AWG { get; set; }

        /// <summary>
        /// Площадь поперечного сечения (мм²)
        /// </summary>
        public double CrossSectionMm2 { get; set; }

        /// <summary>
        /// Максимальный длительный ток (А)
        /// </summary>
        public double MaxCurrentAmp { get; set; }

        /// <summary>
        /// Сопротивление на метр длины (Ом/м)
        /// </summary>
        public double ResistancePerMeter { get; set; }

        #endregion

        #region methods

        /// <summary>
        /// Рассчитать падение напряжения на проводе
        /// </summary>
        /// <param name="current">Ток (А)</param>
        /// <param name="lengthMeters">Длина провода (м)</param>
        /// <returns>Падение напряжения (В)</returns>
        public double CalculateVoltageDrop(double current, double lengthMeters)
        {
            double totalResistance = ResistancePerMeter * lengthMeters;
            return current * totalResistance;
        }

        /// <summary>
        /// Проверить, подходит ли провод для заданного тока
        /// </summary>
        /// <param name="current">Ток (А)</param>
        /// <returns>true если ток не превышает 80% от максимального</returns>
        public bool IsSuitableForCurrent(double current)
        {
            return current <= MaxCurrentAmp * 0.8;
        }

        /// <summary>
        /// Рассчитать потери мощности в проводе
        /// </summary>
        /// <param name="current">Ток (А)</param>
        /// <param name="lengthMeters">Длина провода (м)</param>
        /// <returns>Потери мощности (Вт)</returns>
        public double CalculatePowerLoss(double current, double lengthMeters)
        {
            double voltageDrop = CalculateVoltageDrop(current, lengthMeters);
            return voltageDrop * current;
        }

        /// <summary>
        /// Рассчитать эффективность передачи энергии через провод
        /// </summary>
        /// <param name="current">Ток (А)</param>
        /// <param name="lengthMeters">Длина провода (м)</param>
        /// <param name="systemVoltage">Напряжение системы (В)</param>
        /// <returns>КПД (0.0-1.0)</returns>
        public double CalculateEfficiency(double current, double lengthMeters, double systemVoltage)
        {
            double powerLoss = CalculatePowerLoss(current, lengthMeters);
            double totalPower = systemVoltage * current;

            if (totalPower <= 0) return 1.0;

            return 1.0 - (powerLoss / totalPower);
        }

        /// <summary>
        /// Получить статус безопасности для заданного тока
        /// </summary>
        /// <param name="current">Ток (А)</param>
        /// <returns>Текстовое описание уровня безопасности</returns>
        public string GetSafetyStatus(double current)
        {
            if (current > MaxCurrentAmp)
                return "ОПАСНО: Ток превышает максимальный!";
            else if (current > MaxCurrentAmp * 0.8)
                return "ПРЕДУПРЕЖДЕНИЕ: Близко к пределу";
            else
                return "Безопасно";
        }

        /// <summary>
        /// Получить рекомендуемую максимальную длину провода
        /// </summary>
        /// <param name="current">Ток (А)</param>
        /// <param name="maxVoltageDrop">Максимально допустимое падение напряжения (В)</param>
        /// <param name="systemVoltage">Напряжение системы (В)</param>
        /// <returns>Максимальная длина в метрах</returns>
        public double GetRecommendedMaxLength(double current, double maxVoltageDrop, double systemVoltage)
        {
            if (current <= 0) return double.MaxValue;

            double maxResistance = maxVoltageDrop / current;
            return maxResistance / ResistancePerMeter;
        }

        /// <summary>
        /// Рассчитать оптимальный калибр AWG для заданных параметров
        /// </summary>
        /// <param name="current">Ток (А)</param>
        /// <param name="lengthMeters">Длина провода (м)</param>
        /// <param name="systemVoltage">Напряжение системы (В)</param>
        /// <param name="maxDropPercent">Максимальное падение напряжения в процентах</param>
        /// <returns>Рекомендуемый AWG</returns>
        public double CalculateOptimalAWG(double current, double lengthMeters, double systemVoltage, double maxDropPercent = 3.0)
        {
            double maxVoltageDrop = systemVoltage * (maxDropPercent / 100.0);
            double requiredResistance = maxVoltageDrop / current;
            double requiredResistancePerMeter = requiredResistance / lengthMeters;

            var awgTable = new Dictionary<int, double>
            {
                { 6, 0.00130 },
                { 8, 0.00206 },
                { 10, 0.00328 },
                { 12, 0.00521 },
                { 14, 0.00829 }
            };

            foreach (var pair in awgTable.OrderBy(p => p.Key))
            {
                if (pair.Value <= requiredResistancePerMeter)
                    return pair.Key;
            }

            return 6;
        }

        /// <summary>
        /// Создать копию объекта провода
        /// </summary>
        public Wire Clone()
        {
            return new Wire
            {
                AWG = AWG,
                CrossSectionMm2 = CrossSectionMm2,
                MaxCurrentAmp = MaxCurrentAmp,
                ResistancePerMeter = ResistancePerMeter
            };
        }

        #endregion
    }
}
