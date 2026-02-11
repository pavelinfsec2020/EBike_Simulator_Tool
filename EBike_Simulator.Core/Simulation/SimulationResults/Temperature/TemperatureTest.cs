namespace EBike_Simulator.Core.Simulation.SimulationResults.Temperature
{
    /// <summary>
    /// Результат теста температуры
    /// </summary>
    public class TemperatureTest
    {
        #region props

        /// <summary>Температура окружающей среды (°C)</summary>
        public double AmbientTemperature { get; set; }

        /// <summary>Пробег при данной температуре (км)</summary>
        public double Range { get; set; }

        /// <summary>Время движения (часы)</summary>
        public double Time { get; set; }

        /// <summary>Конечная температура батареи (°C)</summary>
        public double BatteryTemperature { get; set; }

        /// <summary>Эффективность (км/ч)</summary>
        public double Efficiency { get; set; }

        #endregion
    }
}
