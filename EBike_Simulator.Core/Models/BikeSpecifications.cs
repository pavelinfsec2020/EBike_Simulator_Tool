using System;
using System.Collections.Generic;
using System.Text;

namespace EBike_Simulator.Core.Models
{
    /// <summary>
    /// Параметры райдера и требования пользователя
    /// </summary>
    public class BikeSpecifications
    {
        #region props
      
        /// <summary>
        /// Вес райдера в килограммах
        /// </summary>
        public double RiderWeight { get; set; }
        
        /// <summary>
        /// Вес байка в килограммах
        /// </summary>
        public double BikeWeight { get; set; }
        
        /// <summary>
        /// Диаметр колес в дюймах
        /// </summary>
        public double WheelDiameter { get; set; }
        
        /// <summary>
        /// Желаемая максимальная скорость в км/ч
        /// </summary>
        public double DesiredMaxSpeed { get; set; }
        
        /// <summary>
        /// Желаемый максимальный пробег в км
        /// </summary>
        public double DesiredMaxRange { get; set; }
        
        /// <summary>
        /// Общий вес системы (велосипед + велосипедист)
        /// </summary>
        public double TotalWeight => RiderWeight + BikeWeight;

        #endregion

        #region methods

        /// <summary>
        /// Получить диаметр колеса в метрах
        /// </summary>
        /// <returns>Диаметр в метрах</returns>
        public double GetWheelDiameterMeters() => WheelDiameter * 0.0254;

        /// <summary>
        /// Получить длину окружности колеса в метрах
        /// </summary>
        /// <returns>Длина окружности в метрах</returns>
        public double GetWheelCircumference() => Math.PI * GetWheelDiameterMeters();

        /// <summary>
        /// Рассчитать требуемую мощность для движения с заданной скоростью
        /// </summary>
        /// <param name="speedKmh">Скорость в км/ч</param>
        /// <param name="gradePercent">Уклон в процентах (по умолчанию 0)</param>
        /// <returns>Требуемая мощность в ваттах</returns>
        public double CalculateRequiredPower(double speedKmh, double gradePercent = 0)
        {
            double speedMs = speedKmh / 3.6;
            double rollingResistance = 0.01 * TotalWeight * 9.81;
            double airResistance = 0.5 * 1.225 * 0.9 * 0.5 * speedMs * speedMs;
            double gradeResistance = 0;

            if (gradePercent != 0)
            {
                double gradeRadians = Math.Atan(gradePercent / 100);
                gradeResistance = TotalWeight * 9.81 * Math.Sin(gradeRadians);
            }

            double totalForce = rollingResistance + airResistance + gradeResistance;
            return (totalForce * speedMs) / 0.8;
        }    

        /// <summary>
        /// Создать копию объекта спецификаций
        /// </summary>
        public BikeSpecifications Clone()
        {
            return new BikeSpecifications
            {
                RiderWeight = RiderWeight,
                BikeWeight = BikeWeight,
                WheelDiameter = WheelDiameter,
                DesiredMaxSpeed = DesiredMaxSpeed,
                DesiredMaxRange = DesiredMaxRange
            };
        }

        /// <summary>
        /// Получить текстовое описание конфигурации
        /// </summary>
        public string GetDescription()
        {
            return $"Вес: {TotalWeight}кг (велосипед: {BikeWeight}кг + райдер: {RiderWeight}кг), " +
                   $"Колеса: {WheelDiameter}\", " +
                   $"Цели: {DesiredMaxSpeed}км/ч, {DesiredMaxRange}км";
        }

        #endregion
    }
}
