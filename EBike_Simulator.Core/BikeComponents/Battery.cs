using System;
using System.Collections.Generic;
using System.Text;

namespace EBike_Simulator.Core.BikeComponents
{
    /// <summary>
    /// Модель литий-ионного аккумулятора с учетом температуры и нагрузки
    /// </summary>
    public class Battery
    {
        #region fields

        private double _charge;
        private readonly double _capacity;
        private double _temperature = 20.0;

        #endregion

        #region ctor

        /// <summary>
        /// Создает новый экземпляр аккумулятора
        /// </summary>
        /// <param name="capacity">Номинальная емкость в Ач (константа)</param>
        /// <param name="nominalVoltage">Номинальное напряжение в В (константа, по умолчанию 48В)</param>
        public Battery(double capacity, double nominalVoltage = 48.0)
        {
            _capacity = capacity;
            _charge = Capacity;
            NominalVoltage = nominalVoltage;
            Voltage = nominalVoltage;
        }

        #endregion

        #region props

        /// <summary>
        /// Номинальное напряжение батареи (константа, В)
        /// </summary>
        public double NominalVoltage { get; }

        /// <summary>
        /// Текущее напряжение под нагрузкой (В) - ИЗМЕНЯЕТСЯ в процессе разряда
        /// </summary>
        public double Voltage { get; private set; }

        /// <summary>
        /// Номинальная емкость (константа, Ач)
        /// </summary>
        public double Capacity => _capacity;

        /// <summary>
        /// Максимальный допустимый ток разряда (А)
        /// </summary>
        public double MaxCurrent { get; set; }

        /// <summary>
        /// Текущий остаточный заряд (Ач)
        /// </summary>
        public double Charge => _charge;

        /// <summary>
        /// Уровень заряда в процентах от эффективной емкости при текущей температуре
        /// </summary>
        public double SOC => (_charge / GetEffectiveCapacity()) * 100;

        /// <summary>
        /// Текущая температура батареи (°C)
        /// </summary>
        public double Temperature => _temperature;

        /// <summary>
        /// Текущее внутреннее сопротивление с учетом температуры (Ом)
        /// </summary>
        public double InternalResistance => GetInternalResistance();

        /// <summary>
        /// Напряжение холостого хода без нагрузки (В) - зависит только от SOC
        /// </summary>
        public double OpenCircuitVoltage => CalculateOpenCircuitVoltage();

        #endregion

        #region private methods

        /// <summary>
        /// Рассчитать напряжение холостого хода по кривой разряда Li-ion
        /// </summary>
        private double CalculateOpenCircuitVoltage()
        {
            double socRatio = _charge / GetEffectiveCapacity();

            // Нелинейная кривая разряда для Li-ion ячеек
            if (socRatio > 0.9)
                return NominalVoltage * 1.1;      // 52.8V при 100%
            else if (socRatio > 0.7)
                return NominalVoltage * 1.05;     // 50.4V при 80%
            else if (socRatio > 0.5)
                return NominalVoltage * 1.0;      // 48.0V при 60%
            else if (socRatio > 0.3)
                return NominalVoltage * 0.95;     // 45.6V при 40%
            else if (socRatio > 0.1)
                return NominalVoltage * 0.9;      // 43.2V при 20%
            else
                return NominalVoltage * 0.85;      // 40.8V при 0%
        }

        #endregion

        #region public methods

        /// <summary>
        /// Получить эффективную емкость с учетом текущей температуры
        /// </summary>
        /// <returns>Доступная емкость в Ач (снижается при холоде и жаре)</returns>
        public double GetEffectiveCapacity()
        {
            double temperatureFactor = 1.0;

            if (_temperature <= 0)
            {
                temperatureFactor = 0.5 + (_temperature + 20) * 0.01;
            }
            else if (_temperature <= 25)
            {
                temperatureFactor = 0.7 + (_temperature * 0.012);
            }
            else if (_temperature <= 40)
            {
                temperatureFactor = 1.0 - ((_temperature - 25) * 0.00333);
            }
            else
            {
                temperatureFactor = 0.95 - ((_temperature - 40) * 0.0075);
            }

            return Capacity * Math.Max(0.3, Math.Min(1.0, temperatureFactor));
        }

        /// <summary>
        /// Получить внутреннее сопротивление с учетом температуры
        /// </summary>
        /// <returns>Сопротивление в Ом (растет на холоде и при перегреве)</returns>
        public double GetInternalResistance()
        {
            double baseResistance = 0.05; // 50 мОм при 25°C

            if (_temperature <= 0)
            {
                return baseResistance * (2.0 - (_temperature / 20));
            }
            else if (_temperature <= 25)
            {
                return baseResistance * (1.5 - (_temperature * 0.02));
            }
            else
            {
                return baseResistance * (1.0 + ((_temperature - 25) * 0.01));
            }
        }

        /// <summary>
        /// Получить максимально допустимый ток разряда с учетом температуры
        /// </summary>
        /// <returns>Ток в А (ограничивается при экстремальных температурах)</returns>
        public double GetMaxDischargeCurrent()
        {
            double baseCurrent = MaxCurrent;

            if (_temperature < 0)
                return baseCurrent * 0.5;
            else if (_temperature < 10)
                return baseCurrent * 0.7;
            else if (_temperature > 45)
                return baseCurrent * 0.6;
            else if (_temperature > 35)
                return baseCurrent * 0.8;

            return baseCurrent;
        }

        /// <summary>
        /// Разрядить батарею заданным током в течение времени
        /// </summary>
        /// <param name="current">Ток разряда (А)</param>
        /// <param name="timeHours">Время разряда в часах</param>
        /// <param name="ambientTemperature">Температура окружающей среды (°C)</param>
        public void Use(double current, double timeHours, double ambientTemperature)
        {
            // Обновляем температуру батареи
            UpdateTemperature(current, ambientTemperature, timeHours);

            // Ограничиваем ток по температурным условиям
            double effectiveCurrent = Math.Min(current, GetMaxDischargeCurrent());

            // Рассчитываем падение напряжения на внутреннем сопротивлении
            double resistance = GetInternalResistance();
            double voltageDrop = effectiveCurrent * resistance;

            // Обновляем рабочее напряжение под нагрузкой
            Voltage = Math.Max(OpenCircuitVoltage - voltageDrop, NominalVoltage * 0.7);

            // Потери энергии на нагрев
            double resistanceLoss = effectiveCurrent * effectiveCurrent * resistance;
            double additionalEnergy = resistanceLoss * timeHours / NominalVoltage;

            // Расход заряда с учетом дополнительных потерь
            double used = (effectiveCurrent + additionalEnergy) * timeHours;
            _charge = Math.Max(0, _charge - used);
        }

        /// <summary>
        /// Получить напряжение при заданном токе нагрузки
        /// </summary>
        /// <param name="current">Ток нагрузки (А)</param>
        /// <returns>Напряжение под нагрузкой (В)</returns>
        public double GetVoltageUnderLoad(double current)
        {
            double ocv = CalculateOpenCircuitVoltage();
            double voltageDrop = current * GetInternalResistance();
            return Math.Max(ocv - voltageDrop, NominalVoltage * 0.7);
        }

        /// <summary>
        /// Обновить температуру батареи на основе тока и условий окружающей среды
        /// </summary>
        private void UpdateTemperature(double current, double ambientTemperature, double timeHours)
        {
            double resistance = GetInternalResistance();
            double heatingPower = current * current * resistance;

            double coolingRate = 0.5;
            double tempChange = (heatingPower * timeHours * 3600 * 0.0001) +
                              (ambientTemperature - _temperature) * coolingRate * timeHours * 10;

            _temperature += tempChange;
            _temperature = Math.Clamp(_temperature, -20, 60);
        }

        /// <summary>
        /// Проверить, остался ли заряд в батарее
        /// </summary>
        /// <returns>true если заряд больше 0.1 Ач</returns>
        public bool HasCharge() => _charge > 0.1;

        /// <summary>
        /// Сбросить состояние батареи к начальным значениям (полный заряд, 20°C)
        /// </summary>
        public void Reset()
        {
            _charge = Capacity;
            _temperature = 20.0;
            Voltage = NominalVoltage;
        }

        /// <summary>
        /// Получить влияние текущей температуры на пробег в процентах
        /// </summary>
        /// <returns>Процент потери пробега из-за температуры</returns>
        public double GetTemperatureImpactOnRange()
        {
            double optimalTemp = 25.0;
            double tempDiff = Math.Abs(_temperature - optimalTemp);

            if (tempDiff < 5) return 0;
            else if (tempDiff < 15) return tempDiff * 0.5;
            else return 15 + (tempDiff - 15) * 1.0;
        }

        /// <summary>
        /// Получить доступную энергию в ватт-часах
        /// </summary>
        /// <returns>Энергия = заряд * номинальное напряжение (Вт·ч)</returns>
        public double GetAvailableEnergy()
        {
            return _charge * NominalVoltage;
        }

        /// <summary>
        /// Оценить возможный пробег при заданной средней мощности
        /// </summary>
        /// <param name="averagePower">Средняя потребляемая мощность (Вт)</param>
        /// <returns>Оценочный пробег в км</returns>
        public double EstimateRange(double averagePower)
        {
            if (averagePower <= 0) return 0;
            return (GetAvailableEnergy() / averagePower) * (GetEffectiveCapacity() / Capacity) * 0.8;
        }

        /// <summary>
        /// Проверить, находится ли температура в безопасном диапазоне
        /// </summary>
        /// <returns>true если температура между 0°C и 45°C</returns>
        public bool IsSafeTemperature()
        {
            return _temperature >= 0 && _temperature <= 45;
        }

        /// <summary>
        /// Получить текстовый статус температуры
        /// </summary>
        public string GetTemperatureStatus()
        {
            if (_temperature < 0) return "Критически холодно";
            else if (_temperature < 10) return "Холодно";
            else if (_temperature > 50) return "Критически жарко";
            else if (_temperature > 40) return "Жарко";
            else return "Оптимально";
        }

        /// <summary>
        /// Рассчитать оставшееся время работы при заданном токе
        /// </summary>
        /// <param name="current">Ток разряда (А)</param>
        /// <returns>Время в часах</returns>
        public double CalculateRemainingTime(double current)
        {
            if (current <= 0) return 0;
            return _charge / current;
        }

        /// <summary>
        /// Создать копию объекта батареи с теми же параметрами
        /// </summary>
        public Battery Clone()
        {
            return new Battery(Capacity, NominalVoltage)
            {
                MaxCurrent = MaxCurrent,
                Voltage = Voltage
            };
        }

        #endregion
    }
}
