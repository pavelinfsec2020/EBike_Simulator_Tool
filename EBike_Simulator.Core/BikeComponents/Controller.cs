namespace EBike_Simulator.Core.BikeComponents
{
    /// <summary>
    /// Модель контроллера электромотора с ограничением тока и тепловой защитой
    /// </summary>
    public class Controller
    {
        #region fields

        private double _temperature = 20.0;

        #endregion

        #region props

        /// <summary>
        /// Название модели контроллера
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Максимальный выходной ток контроллера (А)
        /// </summary>
        public double MaxCurrent { get; set; }

        /// <summary>
        /// Вес контроллера в килограммах
        /// </summary>
        public double Weight { get; set; }

        /// <summary>
        /// Текущая температура контроллера (°C)
        /// </summary>
        public double Temperature => _temperature;

        #endregion

        #region public methods

        /// <summary>
        /// Получить выходной ток в зависимости от положения газа
        /// </summary>
        public double GetOutputCurrent(double throttle, double batteryVoltage)
        {
            double requestedCurrent = MaxCurrent * throttle;
            requestedCurrent = Math.Min(requestedCurrent, MaxCurrent);
            return requestedCurrent * GetThermalFactor();
        }

        private double GetThermalFactor()
        {
            if (_temperature <= 60) return 1.0;
            else if (_temperature <= 70) return 0.8;
            else if (_temperature <= 80) return 0.6;
            else return 0.4;
        }

        /// <summary>
        /// Обновить температуру контроллера
        /// </summary>
        public void UpdateTemp(double current, double ambientTemperature, double time)
        {
            double powerLoss = current * current * 0.1;

            double tempRise = powerLoss * time * 0.02;
            double cooling = (_temperature - ambientTemperature) * time * 0.01;

            _temperature += tempRise - cooling;
            _temperature = Math.Clamp(_temperature, ambientTemperature, 80);
        }

        /// <summary>
        /// Сбросить температуру контроллера
        /// </summary>
        public void Reset()
        {
            _temperature = 20.0;
        }

        /// <summary>
        /// Проверить безопасность заданного тока
        /// </summary>
        public bool IsCurrentSafe(double current)
        {
            return current <= MaxCurrent * GetThermalFactor();
        }

        /// <summary>
        /// Получить текущий КПД контроллера
        /// </summary>
        public double GetEfficiency(double current)
        {
            double baseEfficiency = 0.95;
            double currentFactor = Math.Clamp(1.0 - (current / MaxCurrent) * 0.1, 0.85, 1.0);
            return baseEfficiency * currentFactor * GetThermalFactor();
        }

        /// <summary>
        /// Рассчитать потери мощности в контроллере
        /// </summary>
        public double CalculatePowerLoss(double current)
        {
            double efficiency = GetEfficiency(current);
            double inputPower = current * 48;
            return inputPower * (1 - efficiency);
        }

        /// <summary>
        /// Получить текстовый статус температуры
        /// </summary>
        public string GetTemperatureStatus()
        {
            if (_temperature > 80) return "Критический перегрев";
            else if (_temperature > 70) return "Перегрев";
            else if (_temperature > 60) return "Тепло";
            else return "Норма";
        }

        /// <summary>
        /// Проверить наличие перегрева
        /// </summary>
        public bool IsOverheating() => _temperature > 70;

        /// <summary>
        /// Создать копию объекта контроллера
        /// </summary>
        public Controller Clone()
        {
            return new Controller
            {
                Name = Name,
                MaxCurrent = MaxCurrent,
                Weight = Weight
            };
        }

        #endregion

        #region object override

        public override string ToString()
        {
            return $"{MaxCurrent:F0}A Controller"; 
        }

        #endregion
    }
}
