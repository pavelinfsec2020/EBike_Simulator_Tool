namespace EBike_Simulator.Core.BikeComponents
{
    /// <summary>
    /// Литий-ионный аккумулятор с температурными характеристиками и весом
    /// </summary>
    /// <summary>
    /// Литий-ионный аккумулятор с температурными характеристиками и весом
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
        /// <param name="capacity">Номинальная емкость в Ач</param>
        /// <param name="nominalVoltage">Номинальное напряжение в В</param>
        public Battery(double capacity, double nominalVoltage)
        {
            _capacity = capacity;
            _charge = Capacity;
            NominalVoltage = nominalVoltage;
            Voltage = nominalVoltage;
        }

        #endregion

        #region props

        /// <summary>
        /// Название модели аккумулятора
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Номинальное напряжение батареи (константа, В)
        /// </summary>
        public double NominalVoltage { get; }

        /// <summary>
        /// Текущее напряжение под нагрузкой (В)
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
        /// Вес аккумулятора в килограммах
        /// </summary>
        public double Weight { get; set; }

        /// <summary>
        /// Текущий остаточный заряд (Ач)
        /// </summary>
        public double Charge => _charge;

        /// <summary>
        /// Уровень заряда в процентах от эффективной емкости
        /// </summary>
        public double SOC => (_charge / GetEffectiveCapacity()) * 100;

        /// <summary>
        /// Текущая температура батареи (°C)
        /// </summary>
        public double Temperature => _temperature;

        /// <summary>
        /// Текущее внутреннее сопротивление (Ом)
        /// </summary>
        public double InternalResistance => GetInternalResistance();

        /// <summary>
        /// Напряжение холостого хода (В)
        /// </summary>
        public double OpenCircuitVoltage => CalculateOpenCircuitVoltage();

        #endregion

        #region private methods

        private double CalculateOpenCircuitVoltage()
        {
            double socRatio = _charge / GetEffectiveCapacity();

            if (socRatio > 0.9)
                return NominalVoltage * 1.1;
            else if (socRatio > 0.7)
                return NominalVoltage * 1.05;
            else if (socRatio > 0.5)
                return NominalVoltage * 1.0;
            else if (socRatio > 0.3)
                return NominalVoltage * 0.95;
            else if (socRatio > 0.1)
                return NominalVoltage * 0.9;
            else
                return NominalVoltage * 0.85;
        }

        #endregion

        #region public methods

        /// <summary>
        /// Получить эффективную емкость с учетом температуры
        /// </summary>
        public double GetEffectiveCapacity()
        {
            double temperatureFactor = 1.0;

            if (_temperature < -20)
            {
                temperatureFactor = 0.3; 
            }
            else if (_temperature < -10)
            {
                temperatureFactor = 0.4; 
            }
            else if (_temperature < 0)
            {
                temperatureFactor = 0.5; 
            }
            else if (_temperature < 10)
            {
                temperatureFactor = 0.7; 
            }
            else if (_temperature < 20)
            {
                temperatureFactor = 0.85; 
            }
            else if (_temperature <= 30)
            {
                temperatureFactor = 1.0; 
            }
            else if (_temperature <= 40)
            {
                temperatureFactor = 0.9; 
            }
            else if (_temperature <= 50)
            {
                temperatureFactor = 0.7; 
            }
            else
            {
                temperatureFactor = 0.5; 
            }

            return Capacity * temperatureFactor;
        }

        /// <summary>
        /// Получить внутреннее сопротивление с учетом температуры
        /// </summary>
        public double GetInternalResistance()
        {
            double baseResistance = 0.05; 

            // Сопротивление растет при низких температурах
            if (_temperature < -10)
                return baseResistance * 3.0; 
            else if (_temperature < 0)
                return baseResistance * 2.0;
            else if (_temperature < 10)
                return baseResistance * 1.5; 
            else if (_temperature <= 30)
                return baseResistance; 
            else if (_temperature <= 40)
                return baseResistance * 1.2; 
            else
                return baseResistance * 1.5; 
        }

        /// <summary>
        /// Установить температуру батареи (для тестирования)
        /// </summary>
        public void SetTemperature(double temperature)
        {
            _temperature = Math.Clamp(temperature, -20, 60);
        }

        /// <summary>
        /// Установить температуру батареи (для тестирования)
        /// </summary>
        public void ForceTemperature(double temperature)
        {
            _temperature = Math.Clamp(temperature, -20, 60);
        }

        /// <summary>
        /// Получить максимально допустимый ток разряда
        /// </summary>
        public double GetMaxDischargeCurrent()
        {
            double baseCurrent = MaxCurrent;

            if (_temperature < -10)
                return baseCurrent * 0.3; 
            else if (_temperature < 0)
                return baseCurrent * 0.5; 
            else if (_temperature < 10)
                return baseCurrent * 0.7; 
            else if (_temperature > 45)
                return baseCurrent * 0.5; 
            else if (_temperature > 35)
                return baseCurrent * 0.8; 

            return baseCurrent;
        }

        /// <summary>
        /// Разрядить батарею
        /// </summary>
        public void Use(double current, double timeHours, double ambientTemperature)
        {
            UpdateTemperature(current, ambientTemperature, timeHours);

            double effectiveCurrent = Math.Min(current, GetMaxDischargeCurrent());
            double resistance = GetInternalResistance();
            double voltageDrop = effectiveCurrent * resistance;

            Voltage = Math.Max(OpenCircuitVoltage - voltageDrop, NominalVoltage * 0.7);

            double resistanceLoss = effectiveCurrent * effectiveCurrent * resistance;
            double additionalEnergy = resistanceLoss * timeHours / NominalVoltage;

            double used = (effectiveCurrent + additionalEnergy) * timeHours;
            _charge = Math.Max(0, _charge - used);
        }

        /// <summary>
        /// Получить напряжение при заданном токе нагрузки
        /// </summary>
        public double GetVoltageUnderLoad(double current)
        {
            double ocv = CalculateOpenCircuitVoltage();
            double voltageDrop = current * GetInternalResistance();
            return Math.Max(ocv - voltageDrop, NominalVoltage * 0.7);
        }

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
        /// Проверить, остался ли заряд
        /// </summary>
        public bool HasCharge() => _charge > 0.1;

        /// <summary>
        /// Сбросить состояние батареи
        /// </summary>
        public void Reset()
        {
            _charge = Capacity;
            _temperature = 20.0;
            Voltage = NominalVoltage;
        }

        /// <summary>
        /// Получить влияние температуры на пробег
        /// </summary>
        public double GetTemperatureImpactOnRange()
        {
            double effectiveCapacity = GetEffectiveCapacity();
            double loss = (Capacity - effectiveCapacity) / Capacity * 100;
            return loss;
        }

        /// <summary>
        /// Получить доступную энергию в ватт-часах
        /// </summary>
        public double GetAvailableEnergy()
        {
            return _charge * NominalVoltage;
        }

        /// <summary>
        /// Оценить пробег при заданной средней мощности
        /// </summary>
        public double EstimateRange(double averagePower)
        {
            if (averagePower <= 0) return 0;
            double effectiveEnergy = GetEffectiveCapacity() * NominalVoltage;
            return (effectiveEnergy / averagePower) * 0.9; 
        }

        /// <summary>
        /// Проверить безопасность температуры
        /// </summary>
        public bool IsSafeTemperature()
        {
            return _temperature >= -10 && _temperature <= 45;
        }

        /// <summary>
        /// Получить текстовый статус температуры
        /// </summary>
        public string GetTemperatureStatus()
        {
            if (_temperature < -10) return "Критически холодно";
            else if (_temperature < 0) return "Очень холодно";
            else if (_temperature < 10) return "Холодно";
            else if (_temperature > 45) return "Критически жарко";
            else if (_temperature > 35) return "Жарко";
            else return "Оптимально";
        }

        /// <summary>
        /// Рассчитать оставшееся время работы
        /// </summary>
        public double CalculateRemainingTime(double current)
        {
            if (current <= 0) return 0;
            return _charge / current;
        }

        /// <summary>
        /// Создать копию объекта батареи
        /// </summary>
        public Battery Clone()
        {
            return new Battery(Capacity, NominalVoltage)
            {
                Name = Name,
                MaxCurrent = MaxCurrent,
                Weight = Weight,
                Voltage = Voltage
            };
        }

        #endregion
    }
}
