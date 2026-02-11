using System;
using System.Collections.Generic;
using System.Text;

namespace EBike_Simulator.Core.BikeComponents
{
    /// <summary>
    /// Модель электромотора для велосипеда с учетом тепловых характеристик
    /// </summary>
    public class Motor
    {
        #region fields

        private double _temperature = 20.0;

        #endregion

        #region props

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
        /// Текущая температура мотора (°C)
        /// </summary>
        public double Temperature => _temperature;

        #endregion

        #region methods

        /// <summary>
        /// Получить текущий КПД с учетом температуры
        /// </summary>
        /// <returns>КПД (снижается при перегреве и на морозе)</returns>
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
        /// <param name="throttle">Положение газа (0.0-1.0)</param>
        /// <returns>Мощность на валу (Вт)</returns>
        public double GetOutputPower(double throttle)
        {
            double requestedPower = Power * throttle;
            requestedPower = Math.Min(requestedPower, MaxPower);
            return requestedPower * GetThermalLimitFactor();
        }

        /// <summary>
        /// Обновить температуру мотора на основе потребляемой мощности
        /// </summary>
        /// <param name="power">Потребляемая мощность (Вт)</param>
        /// <param name="ambientTemperature">Температура окружающей среды (°C)</param>
        /// <param name="time">Время работы в секундах</param>
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
        /// Сбросить температуру мотора до температуры окружающей среды
        /// </summary>
        /// <param name="ambientTemperature">Температура окружающей среды (°C)</param>
        public void Reset(double ambientTemperature)
        {
            _temperature = ambientTemperature;
        }

        /// <summary>
        /// Проверить наличие перегрева
        /// </summary>
        /// <returns>true если температура выше 80°C</returns>
        public bool IsOverheating() => _temperature > 80;

        /// <summary>
        /// Получить коэффициент теплового ограничения мощности
        /// </summary>
        /// <returns>Множитель мощности (1.0 - нет ограничений, 0.3 - сильное ограничение)</returns>
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
        /// <param name="power">Требуемая мощность (Вт)</param>
        /// <returns>Ток в амперах</returns>
        public double CalculateRequiredCurrent(double power)
        {
            if (Voltage <= 0) return 0;
            return power / (Voltage * GetEfficiency());
        }

        /// <summary>
        /// Рассчитать крутящий момент на заданных оборотах
        /// </summary>
        /// <param name="rpm">Обороты в минуту</param>
        /// <returns>Крутящий момент в Н·м</returns>
        public double CalculateTorque(double rpm)
        {
            if (rpm <= 0)
                return 0;
           
            double powerWatts = GetOutputPower(1.0);
           
            return (powerWatts * 60) / (2 * Math.PI * rpm);
        }

        /// <summary>
        /// Получить текстовый статус температуры
        /// </summary>
        public string GetTemperatureStatus()
        {
            if (_temperature > 100)
                return "Критический перегрев";
            else if (_temperature > 80) 
                return "Перегрев";
            else if (_temperature > 60) 
                return "Тепло";
            else return "Норма";
        }

        /// <summary>
        /// Проверить возможность выдачи требуемой мощности
        /// </summary>
        /// <param name="requestedPower">Запрашиваемая мощность (Вт)</param>
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
                Voltage = Voltage,
                Power = Power,
                MaxPower = MaxPower,
                Efficiency = Efficiency
            };
        }

        #endregion
    }
}
