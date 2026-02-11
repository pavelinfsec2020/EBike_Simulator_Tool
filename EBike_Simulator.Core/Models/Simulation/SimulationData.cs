using EBike_Simulator.Core.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace EBike_Simulator.Core.Models.Simulation
{
    public class SimulationData
    {
        #region props
        public double Time { get; set; }
        public double Speed { get; set; }
        public double Distance { get; set; }
        public double MotorTemp { get; set; }
        public double ControllerTemp { get; set; }
        public double BatteryTemp { get; set; }
        public double BatterySOC { get; set; }
        public double Current { get; set; }
        public double Power { get; set; }
        public double WindEffect { get; set; }
        public double TempEffect { get; set; }

        public bool IsMotorOverheating() => MotorTemp > 80;
        public bool IsControllerOverheating() => ControllerTemp > 70;

        #endregion

        #region methods
        public ThermalStatus GetThermalStatus()
        {
            if (MotorTemp > 100 || ControllerTemp > 80 || BatteryTemp > 50)
                return ThermalStatus.Critical;
            else if (MotorTemp > 80 || ControllerTemp > 70 || BatteryTemp > 45)
                return ThermalStatus.Warning;
            else
                return ThermalStatus.Normal;
        }

        public double GetInstantEfficiency()
        {
            if (Power <= 0 || Speed <= 0) 
                return 0;
           
            return (Speed * 1000) / (Power + 0.0001);
        }
       
        #endregion
    }
}
