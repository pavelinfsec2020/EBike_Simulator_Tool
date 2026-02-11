using EBike_Simulator.Core.Enums;

namespace EBike_Simulator.Core.Models.Simulation
{
    /// <summary>
    /// Точка данных симуляции
    /// </summary>
    public class SimulationData
    {
        #region props
        /// <summary>Время от начала симуляции (секунды)</summary>
        public double Time { get; set; }
        
        /// <summary>Текущая скорость (км/ч)</summary>
        public double Speed { get; set; }
      
        /// <summary>Пройденное расстояние (км)</summary>
        public double Distance { get; set; }
       
        /// <summary>Температура мотора (°C)</summary>
        public double MotorTemp { get; set; }
        
        /// <summary>Температура контроллера (°C)</summary>
        public double ControllerTemp { get; set; }
       
        /// <summary>Температура батареи (°C)</summary>
        public double BatteryTemp { get; set; }
       
        /// <summary>Уровень заряда батареи (%)</summary>
        public double BatterySOC { get; set; }
       
        /// <summary>Потребляемый ток (А)</summary>
        public double Current { get; set; }
       
        /// <summary>Мощность на моторе (Вт)</summary>
        public double Power { get; set; }
       
        /// <summary>Влияние ветра на текущую точку (%)</summary>
        public double WindEffect { get; set; }
       
        /// <summary>Влияние температуры на текущую точку (%)</summary>
        public double TempEffect { get; set; }
       
        /// <summary>
        /// Проверить перегрев мотора
        /// </summary>
        public bool IsMotorOverheating() => MotorTemp > 80;
       
        /// <summary>
        /// Проверить перегрев контроллера
        /// </summary>
        public bool IsControllerOverheating() => ControllerTemp > 70;
        #endregion

        #region public methods
        /// <summary>
        /// Получить мгновенную энергоэффективность
        /// </summary>
        /// <returns>км/кВт·ч</returns>
        public double GetInstantEfficiency()
        {
            if (Power <= 0 || Speed <= 0) return 0;
            return (Speed * 1000) / (Power + 0.0001);
        }

        /// <summary>
        /// Получить тепловой статус системы
        /// </summary>
        public ThermalStatus GetThermalStatus()
        {
            if (MotorTemp > 100 || ControllerTemp > 80 || BatteryTemp > 50)
                return ThermalStatus.Critical;
            else if (MotorTemp > 80 || ControllerTemp > 70 || BatteryTemp > 45)
                return ThermalStatus.Warning;
            else
                return ThermalStatus.Normal;
        }

        #endregion
    }
}
