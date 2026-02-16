using EBike_Simulator.Core.Helpers;

namespace EBike_Simulator.Core.BikeComponents
{
    /// <summary>
    /// Модель электромотора для велосипеда с учетом тепловых характеристик
    /// </summary>
    /// <summary>
    /// Модель электромотора для велосипеда с учетом тепловых характеристик и веса
    /// </summary>
    public class Motor
    {
        #region fields

        private double _temperature = 20.0;

        #endregion

        #region props

        /// <summary>
        /// Название модели мотора
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Номинальное напряжение мотора (В)
        /// </summary>
        public double Voltage { get; set; }

        /// <summary>
        /// Номинальная мощность мотора (Вт)
        /// </summary>
        public double Power { get; set; }

        /// <summary>
        /// Максимальная мощность мотора (Вт)
        /// </summary>
        public double MaxPower { get; set; }

        /// <summary>
        /// Базовый КПД мотора (0.0-1.0)
        /// </summary>
        public double Efficiency { get; set; } = 0.85;

        /// <summary>
        /// Вес мотора в килограммах
        /// </summary>
        public double Weight { get; set; }

        /// <summary>
        /// Текущая температура мотора (°C)
        /// </summary>
        public double Temperature => _temperature;

        #endregion

        #region public methods

        /// <summary>
        /// Получить текущий КПД с учетом температуры
        /// </summary>
        public double GetEfficiency()
        {
            double baseEfficiency = Efficiency;

            if (_temperature > 80)
                return baseEfficiency * 0.9;
            else if (_temperature > 60)
                return baseEfficiency * 0.95;
            else if (_temperature < 0)
                return baseEfficiency * 0.9;

            return baseEfficiency;
        }

        /// <summary>
        /// Получить выходную мощность при заданном положении ручки газа
        /// </summary>
        public double GetOutputPower(double throttle)
        {
            double requestedPower = Power * Math.Clamp(throttle, 0, 1);
            requestedPower = Math.Min(requestedPower, MaxPower);
            double thermalFactor = GetThermalLimitFactor();

            return requestedPower * thermalFactor;
        }

        /// <summary>
        /// Обновить температуру мотора
        /// </summary>
        public void UpdateTemp(double power, double ambientTemperature, double time)
        {
            double efficiency = GetEfficiency();
            double powerLoss = power * (1 - efficiency);

            double tempRise = powerLoss * time * 0.01;
            double cooling = (_temperature - ambientTemperature) * time * 0.005;

            _temperature += tempRise - cooling;
            _temperature = Math.Clamp(_temperature, ambientTemperature, 120);
        }

        /// <summary>
        /// Сбросить температуру мотора
        /// </summary>
        public void Reset(double ambientTemperature)
        {
            _temperature = ambientTemperature;
        }

        /// <summary>
        /// Проверить наличие перегрева
        /// </summary>
        public bool IsOverheating() => _temperature > 80;

        /// <summary>
        /// Получить коэффициент теплового ограничения
        /// </summary>
        public double GetThermalLimitFactor()
        {
            if (_temperature <= 60) return 1.0;
            else if (_temperature <= 80) return 0.8;
            else if (_temperature <= 100) return 0.5;
            else return 0.3;
        }

        /// <summary>
        /// Рассчитать требуемый ток для заданной мощности
        /// </summary>
        public double CalculateRequiredCurrent(double power)
        {
            if (Voltage <= 0) return 0;
            return power / (Voltage * GetEfficiency());
        }

        /// <summary>
        /// Рассчитать крутящий момент
        /// </summary>
        public double CalculateTorque(double rpm)
        {
            if (rpm <= 0) return 0;
            double powerWatts = GetOutputPower(1.0);
            return (powerWatts * 60) / (2 * Math.PI * rpm);
        }

        /// <summary>
        /// Получить текстовый статус температуры
        /// </summary>
        public string GetTemperatureStatus()
        {
            var key = "standard";

            if (_temperature > 80) key = "criticalOverheating";
            else if (_temperature > 70) key = "overheating";
            else if (_temperature > 60) key = "warm";

            return Translater.TranslateByKey(key);
        }

        /// <summary>
        /// Проверить возможность выдачи требуемой мощности
        /// </summary>
        public bool CanDeliverPower(double requestedPower)
        {
            return requestedPower <= MaxPower * GetThermalLimitFactor();
        }

        /// <summary>
        /// Создать копию объекта мотора
        /// </summary>
        public Motor Clone()
        {
            return new Motor
            {
                Name = Name,
                Voltage = Voltage,
                Power = Power,
                MaxPower = MaxPower,
                Efficiency = Efficiency,
                Weight = Weight
            };
        }

        #endregion
    }
}
