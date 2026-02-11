using System;
using System.Collections.Generic;
using System.Text;

namespace EBike_Simulator.Core.Models
{
    public class Environment
    {
        #region props

        public double Temperature { get; set; } = 20.0; //  (°C)
        public Wind WindParams { get; set; } = new Wind();

        #endregion

        #region methods

        public double GetTemperatureImpactOnBattery()
        {
            if (Temperature <= 0)
                return 0.5 + (Temperature + 20) * 0.025;
            else if (Temperature <= 25)
                return 0.7 + (Temperature * 0.012);
            else if (Temperature <= 40)
                return 1.0 - ((Temperature - 25) * 0.01);
            else
                return 0.85 - ((Temperature - 40) * 0.005);
        }

        public Environment Clone()
        {
            return new Environment
            {
                Temperature = Temperature,
                WindParams = WindParams.Clone()
            };
        }

        #endregion
    }
}
