using EBike_Simulator.Core.Enums;

namespace EBike_Simulator.Core.Simulation.SimulationResults.Wind
{
    /// <summary>
    /// Результат теста ветра
    /// </summary>
    public class WindImpactResult
    {
        #region props

        /// <summary>Направление ветра</summary>
        public WindDirection Direction { get; set; }

        /// <summary>Скорость ветра (м/с)</summary>
        public double WindSpeed { get; set; }

        /// <summary>Пробег при данном ветре (км)</summary>
        public double Range { get; set; }

        /// <summary>Время движения (часы)</summary>
        public double Time { get; set; }

        /// <summary>Эффективность (км/ч)</summary>
        public double Efficiency { get; set; }

        #endregion
    }
}

