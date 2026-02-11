using EBike_Simulator.Core.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace EBike_Simulator.Core.Models
{
    public class Wind
    {
        #region props

        public double Speed { get; set; } // m/s
        public WindDirection Direction { get; set; }

        #endregion

        #region methods

        public double GetEffectiveWindForce(double bikeSpeed)
        {
            double bikeSpeedMs = bikeSpeed / 3.6;

            double effectiveWindSpeed = Direction switch
            {
                WindDirection.Headwind => bikeSpeedMs + Speed,
                WindDirection.Tailwind => bikeSpeedMs - Speed,
                WindDirection.Crosswind => bikeSpeedMs,
                _ => bikeSpeedMs
            };

            return CalculateAirResistance(effectiveWindSpeed);
        }

        private double CalculateAirResistance(double speedMs)
        {
            const double AirDensity = 1.225; // kg/m³
            const double DragCoefficient = 0.9;
            const double FrontalArea = 0.5; // m²

            if (speedMs <= 0) return 0;

            return 0.5 * AirDensity * DragCoefficient * FrontalArea * speedMs * speedMs;
        }

        public string GetDirectionName()
        {
            return Direction switch
            {
                WindDirection.Headwind => "headwind",
                WindDirection.Tailwind => "tailwind",
                WindDirection.Crosswind => "crosswind",
                _ => "none"
            };
        }

        public double GetImpactPercentage(double bikeSpeed)
        {
            double noWindResistance = CalculateAirResistance(bikeSpeed / 3.6);
            double withWindResistance = GetEffectiveWindForce(bikeSpeed);

            if (noWindResistance <= 0) return 0;

            return ((withWindResistance - noWindResistance) / noWindResistance) * 100;
        }

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
