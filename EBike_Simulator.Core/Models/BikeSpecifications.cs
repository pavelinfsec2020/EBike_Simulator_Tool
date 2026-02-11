using System;
using System.Collections.Generic;
using System.Text;

namespace EBike_Simulator.Core.Models
{
    public class BikeSpecifications
    {
        #region props
      
        public double RiderWeight { get; set; } // kg
        public double BikeWeight { get; set; } // kg
        public double WheelDiameter { get; set; } // inch
        public double DesiredMaxSpeed { get; set; } // km/h
        public double DesiredMaxRange { get; set; } // km
        public double TotalWeight => RiderWeight + BikeWeight;

        #endregion

        #region methods

        public double GetWheelDiameterMeters() => WheelDiameter * 0.0254;
        public double GetWheelCircumference() => Math.PI * GetWheelDiameterMeters();

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

        public string GetDescription()
        {
            return $"Вес: {TotalWeight}кг (велосипед: {BikeWeight}кг + райдер: {RiderWeight}кг), " +
                   $"Колеса: {WheelDiameter}\", " +
                   $"Цели: {DesiredMaxSpeed}км/ч, {DesiredMaxRange}км";
        }

        #endregion
    }
}
