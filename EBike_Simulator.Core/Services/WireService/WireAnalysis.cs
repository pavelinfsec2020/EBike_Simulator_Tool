using EBike_Simulator.Core.BikeComponents;
using System;
using System.Collections.Generic;
using System.Text;

namespace EBike_Simulator.Core.Services.WireService
{
    /// <summary>
    /// Результат анализа для конкретной длины провода
    /// </summary>
    public class WireAnalysis
    {
        #region props

        /// <summary>Длина провода (м)</summary>
        public double Length { get; set; }

        /// <summary>Выбранный провод</summary>
        public Wire SelectedWire { get; set; }

        /// <summary>Падение напряжения (В)</summary>
        public double VoltageDrop { get; set; }

        /// <summary>Падение напряжения в процентах</summary>
        public double VoltageDropPercent { get; set; }

        /// <summary>Потери мощности (Вт)</summary>
        public double PowerLoss { get; set; }

        /// <summary>Эффективность передачи</summary>
        public double Efficiency { get; set; }

        /// <summary>Статус безопасности</summary>
        public string SafetyStatus { get; set; }

        #endregion
    }
}
