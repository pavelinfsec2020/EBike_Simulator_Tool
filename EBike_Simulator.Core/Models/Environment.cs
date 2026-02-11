using System;
using System.Collections.Generic;
using System.Text;

namespace EBike_Simulator.Core.Models
{
    /// <summary>
    /// Погодные условия окружающей среды
    /// </summary>
    public class Environment
    {
        #region props

        /// <summary>
        /// Температура воздуха в градусах Цельсия
        /// </summary>
        public double Temperature { get; set; } = 20.0;

        /// <summary>
        /// Параметры ветра
        /// </summary>
        public Wind Wind { get; set; } = new Wind();

        #endregion

        #region public methods

        /// <summary>
        /// Получить коэффициент влияния температуры на емкость батареи
        /// </summary>
        /// <returns>Множитель емкости (1.0 = 100% при 25°C)</returns>
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

        /// <summary>
        /// Создать копию объекта погодных условий
        /// </summary>
        public Environment Clone()
        {
            return new Environment
            {
                Temperature = Temperature,
                Wind = Wind.Clone()
            };
        }

        #endregion
    }
}
