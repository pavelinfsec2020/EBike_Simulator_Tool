using EBike_Simulator.Core.BikeComponents;
using System;
using System.Collections.Generic;
using System.Text;

namespace EBike_Simulator.Core.Services.WireService
{
    /// <summary>
    /// Сервис для подбора оптимального сечения проводов (AWG) на основе тока и длины
    /// </summary>
    public class WireSelector
    {
        #region Private Fields

        /// <summary>
        /// Таблица стандартных медных проводов с характеристиками
        /// </summary>
        private readonly List<Wire> _wireTable = new()
        {
            new Wire { AWG = 4, CrossSectionMm2 = 21.15, MaxCurrentAmp = 150, ResistancePerMeter = 0.00081 },
            new Wire { AWG = 6, CrossSectionMm2 = 13.30, MaxCurrentAmp = 101, ResistancePerMeter = 0.00129 },
            new Wire { AWG = 8, CrossSectionMm2 = 8.37, MaxCurrentAmp = 73, ResistancePerMeter = 0.00205 },
            new Wire { AWG = 10, CrossSectionMm2 = 5.26, MaxCurrentAmp = 55, ResistancePerMeter = 0.00328 },
            new Wire { AWG = 12, CrossSectionMm2 = 3.31, MaxCurrentAmp = 41, ResistancePerMeter = 0.00521 },
            new Wire { AWG = 14, CrossSectionMm2 = 2.08, MaxCurrentAmp = 32, ResistancePerMeter = 0.00829 },
            new Wire { AWG = 16, CrossSectionMm2 = 1.31, MaxCurrentAmp = 22, ResistancePerMeter = 0.0132 },
            new Wire { AWG = 18, CrossSectionMm2 = 0.82, MaxCurrentAmp = 16, ResistancePerMeter = 0.0210 }
        };

        #endregion

        #region Public Methods

        /// <summary>
        /// Подобрать оптимальный провод по току, длине и напряжению
        /// </summary>
        /// <param name="maxCurrent">Максимальный ток (А)</param>
        /// <param name="wireLength">Длина провода (м)</param>
        /// <param name="systemVoltage">Напряжение системы (В)</param>
        /// <param name="maxVoltageDropPercent">Максимальное падение напряжения в процентах (по умолч. 3%)</param>
        /// <returns>Рекомендованный провод</returns>
        /// <exception cref="ArgumentException">Выбрасывается при некорректных параметрах</exception>
        public Wire SelectWire(
            double maxCurrent,
            double wireLength,
            double systemVoltage,
            double maxVoltageDropPercent = 3.0)
        {
            if (maxCurrent <= 0)
                throw new ArgumentException("Ток должен быть больше 0", nameof(maxCurrent));
            if (wireLength <= 0)
                throw new ArgumentException("Длина провода должна быть больше 0", nameof(wireLength));
            if (systemVoltage <= 0)
                throw new ArgumentException("Напряжение системы должно быть больше 0", nameof(systemVoltage));

            double maxVoltageDrop = systemVoltage * (maxVoltageDropPercent / 100.0);
            Wire selectedWire = null;

            // Перебираем провода от самых толстых к тонким
            foreach (var wire in _wireTable.OrderBy(w => w.AWG))
            {
                if (!wire.IsSuitableForCurrent(maxCurrent))
                    continue;

                double voltageDrop = wire.CalculateVoltageDrop(maxCurrent, wireLength);
                if (voltageDrop <= maxVoltageDrop)
                {
                    selectedWire = wire;
                    break;
                }
            }

            // Если не нашли - возвращаем самый толстый
            return selectedWire ?? _wireTable.OrderBy(w => w.AWG).First();
        }

        /// <summary>
        /// Подобрать провода для всей силовой цепи велосипеда
        /// </summary>
        /// <param name="batteryToControllerCurrent">Ток от батареи к контроллеру (А)</param>
        /// <param name="controllerToMotorCurrent">Ток от контроллера к мотору (А)</param>
        /// <param name="batteryToControllerLength">Длина провода до контроллера (м)</param>
        /// <param name="controllerToMotorLength">Длина провода до мотора (м)</param>
        /// <param name="systemVoltage">Напряжение системы (В)</param>
        /// <param name="maxVoltageDropPercent">Максимальное падение напряжения в процентах</param>
        /// <returns>Словарь с рекомендованными проводами для каждого участка</returns>
        public Dictionary<string, Wire> SelectWiring(
            double batteryToControllerCurrent,
            double controllerToMotorCurrent,
            double batteryToControllerLength = 0.5,
            double controllerToMotorLength = 1.0,
            double systemVoltage = 48,
            double maxVoltageDropPercent = 3.0)
        {
            var wiring = new Dictionary<string, Wire>();

            wiring["Battery_to_Controller"] = SelectWire(
                batteryToControllerCurrent,
                batteryToControllerLength,
                systemVoltage,
                maxVoltageDropPercent
            );

            wiring["Controller_to_Motor"] = SelectWire(
                controllerToMotorCurrent,
                controllerToMotorLength,
                systemVoltage,
                maxVoltageDropPercent
            );

            wiring["Charging_Cable"] = SelectWire(
                10, // Ток зарядки
                2.0, // Длина кабеля зарядки
                systemVoltage,
                maxVoltageDropPercent
            );

            return wiring;
        }

        /// <summary>
        /// Полный анализ системы проводки для различных длин
        /// </summary>
        /// <param name="maxCurrent">Максимальный ток (А)</param>
        /// <param name="wireLengths">Массив длин для анализа</param>
        /// <param name="systemVoltage">Напряжение системы (В)</param>
        /// <param name="maxVoltageDropPercent">Максимальное падение напряжения в процентах</param>
        public WireSelectionReport AnalyzeWiringSystem(
            double maxCurrent,
            double[] wireLengths,
            double systemVoltage = 48,
            double maxVoltageDropPercent = 3.0)
        {
            var report = new WireSelectionReport();

            foreach (double length in wireLengths)
            {
                var wire = SelectWire(maxCurrent, length, systemVoltage, maxVoltageDropPercent);

                double voltageDrop = wire.CalculateVoltageDrop(maxCurrent, length);
                double powerLoss = wire.CalculatePowerLoss(maxCurrent, length);
                double efficiency = wire.CalculateEfficiency(maxCurrent, length, systemVoltage);

                report.Wires.Add(new WireAnalysis
                {
                    Length = length,
                    SelectedWire = wire,
                    VoltageDrop = voltageDrop,
                    VoltageDropPercent = (voltageDrop / systemVoltage) * 100,
                    PowerLoss = powerLoss,
                    Efficiency = efficiency,
                    SafetyStatus = wire.GetSafetyStatus(maxCurrent)
                });
            }

            return report;
        }

        /// <summary>
        /// Получить текстовую рекомендацию по выбору провода
        /// </summary>
        public string GetWireRecommendation(double current, double length, double voltage)
        {
            var wire = SelectWire(current, length, voltage);

            return $@"=== РЕКОМЕНДАЦИЯ ПО ПРОВОДУ ===

Параметры:
• Ток: {current:F1} А
• Длина: {length:F2} м
• Напряжение: {voltage} В

Рекомендованный провод:
• AWG: {wire.AWG}
• Сечение: {wire.CrossSectionMm2:F2} мм²
• Макс. ток: {wire.MaxCurrentAmp} А

Расчеты:
• Падение напряжения: {wire.CalculateVoltageDrop(current, length):F3} В
• Потери мощности: {wire.CalculatePowerLoss(current, length):F1} Вт
• Эффективность: {wire.CalculateEfficiency(current, length, voltage):P1}";
        }

        /// <summary>
        /// Получить список альтернативных проводов, подходящих по параметрам
        /// </summary>
        public List<Wire> GetAlternativeWires(double current, double length, double voltage)
        {
            var alternatives = new List<Wire>();
            double maxVoltageDrop = voltage * 0.03; // 3%

            foreach (var wire in _wireTable)
            {
                if (wire.IsSuitableForCurrent(current))
                {
                    double voltageDrop = wire.CalculateVoltageDrop(current, length);
                    if (voltageDrop <= maxVoltageDrop)
                    {
                        alternatives.Add(wire.Clone());
                    }
                }
            }

            return alternatives.OrderBy(w => w.AWG).ToList();
        }

        /// <summary>
        /// Проверить, подходит ли текущая проводка
        /// </summary>
        public bool ValidateWiring(Dictionary<string, Wire> wiring, Dictionary<string, double> currents)
        {
            foreach (var kvp in wiring)
            {
                if (currents.ContainsKey(kvp.Key))
                {
                    double current = currents[kvp.Key];
                    if (!kvp.Value.IsSuitableForCurrent(current))
                        return false;
                }
            }
            return true;
        }

        #endregion
    }

}
