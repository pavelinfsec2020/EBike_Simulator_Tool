using EBike_Simulator.Core.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace EBike_Simulator.Core.Models
{
    /// <summary>
    /// Параметры ветра
    /// </summary>
    public class Wind
    {
        #region props

        /// <summary>
        /// Скорость ветра в метрах в секунду
        /// </summary>
        public double Speed { get; set; }

        /// <summary>
        /// Направление ветра
        /// </summary>
        public WindDirection Direction { get; set; } = WindDirection.Headwind;

        #endregion

        #region methods

        /// <summary>
        /// Рассчитать силу аэродинамического сопротивления с учетом ветра
        /// </summary>
        /// <param name="bikeSpeed">Скорость велосипеда в км/ч</param>
        /// <returns>Сила сопротивления в ньютонах</returns>
        public double GetEffectiveWindForce(double bikeSpeed)
        {
            double bikeSpeedMs = bikeSpeed / 3.6;
            double effectiveSpeed = Direction switch
            {
                WindDirection.Headwind => bikeSpeedMs + Speed,
                WindDirection.Tailwind => bikeSpeedMs - Speed,
                WindDirection.Crosswind => bikeSpeedMs,
                _ => bikeSpeedMs
            };

            const double AirDensity = 1.225;
            const double DragCoefficient = 0.9;
            const double FrontalArea = 0.5;

            return 0.5 * AirDensity * DragCoefficient * FrontalArea * effectiveSpeed * effectiveSpeed;
        }

        /// <summary>
        /// Получить название направления ветра
        /// </summary>
        public string GetDirectionName()
        {
            return Direction switch
            {
                WindDirection.Headwind => "встречный",
                WindDirection.Tailwind => "попутный",
                WindDirection.Crosswind => "боковой",
                _ => "отсутствует"
            };
        }

        /// <summary>
        /// Рассчитать влияние ветра на сопротивление воздуха в процентах
        /// </summary>
        /// <param name="bikeSpeed">Скорость велосипеда в км/ч</param>
        /// <returns>Процент изменения сопротивления</returns>
        public double GetImpactPercentage(double bikeSpeed)
        {
            const double AirDensity = 1.225;
            const double DragCoefficient = 0.9;
            const double FrontalArea = 0.5;

            double speedMs = bikeSpeed / 3.6;
            double noWindResistance = 0.5 * AirDensity * DragCoefficient * FrontalArea * speedMs * speedMs;
            double withWindResistance = GetEffectiveWindForce(bikeSpeed);

            if (noWindResistance <= 0) return 0;
            return ((withWindResistance - noWindResistance) / noWindResistance) * 100;
        }

        /// <summary>
        /// Создать копию объекта ветра
        /// </summary>
        public Wind Clone()
        {
            return new Wind
            {
                Speed = Speed,
                Direction = Direction
            };
        }

        #endregion
    }
}
