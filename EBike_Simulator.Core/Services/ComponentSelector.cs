using EBike_Simulator.Core.BikeComponents;
using EBike_Simulator.Core.Models;

namespace EBike_Simulator.Core.Services
{
    /// <summary>
    /// Сервис для автоматического подбора компонентов электровелосипеда
    /// на основе требований пользователя
    /// </summary>
    public class ComponentSelector
    {
        #region Private Methods

        /// <summary>
        /// Рассчитать требуемую мощность для достижения желаемой скорости
        /// </summary>
        private double CalculateRequiredPower(BikeSpecifications specs)
        {
            double speedMs = specs.DesiredMaxSpeed / 3.6;
            double rollingResistance = 0.01 * specs.TotalWeight * 9.81;
            double airResistance = 0.5 * 1.225 * 0.9 * 0.5 * speedMs * speedMs;
            double totalForce = rollingResistance + airResistance;
            double requiredPower = totalForce * speedMs;

            // Учет КПД системы (80%) и запас 20%
            return (requiredPower / 0.8) * 1.2;
        }

        /// <summary>
        /// Рассчитать требуемую емкость аккумулятора для желаемого пробега
        /// </summary>
        private double CalculateRequiredCapacity(BikeSpecifications specs, double power)
        {
            // Предполагаем среднюю скорость 80% от максимальной
            double averageSpeedKmh = specs.DesiredMaxSpeed * 0.8;
            double averageSpeedMs = averageSpeedKmh / 3.6;

            double rollingResistance = 0.01 * specs.TotalWeight * 9.81;
            double airResistance = 0.5 * 1.225 * 0.9 * 0.5 * averageSpeedMs * averageSpeedMs;
            double totalForce = rollingResistance + airResistance;

            double averagePower = totalForce * averageSpeedMs / 0.8;

            // Время работы на желаемый пробег
            double operatingHours = specs.DesiredMaxRange / averageSpeedKmh;

            // Требуемая энергия (Вт·ч)
            double requiredEnergy = averagePower * operatingHours;

            // Емкость для 48В системы с запасом 25%
            return (requiredEnergy / 48) * 1.25;
        }

        /// <summary>
        /// Подобрать мотор из доступных вариантов
        /// </summary>
        private Motor SelectMotor(double requiredPower)
        {
            var availableMotors = new[]
            {
                new { Name = "Bafang BBS01B", Voltage = 36.0, Power = 250.0, MaxPower = 350.0, Efficiency = 0.82 },
                new { Name = "Bafang BBS02B", Voltage = 48.0, Power = 500.0, MaxPower = 750.0, Efficiency = 0.85 },
                new { Name = "Bafang BBSHD", Voltage = 48.0, Power = 750.0, MaxPower = 1200.0, Efficiency = 0.87 },
                new { Name = "Tongsheng TSDZ2", Voltage = 36.0, Power = 350.0, MaxPower = 500.0, Efficiency = 0.83 },
                new { Name = "MXUS 3000W", Voltage = 72.0, Power = 1500.0, MaxPower = 3000.0, Efficiency = 0.88 }
            };

            // Выбираем мотор с запасом 20%
            double requiredWithMargin = requiredPower * 1.2;
            var selected = availableMotors.FirstOrDefault(m => m.Power >= requiredWithMargin) ??
                          availableMotors.OrderByDescending(m => m.Power).First();

            return new Motor
            {
                Voltage = selected.Voltage,
                Power = selected.Power,
                MaxPower = selected.MaxPower,
                Efficiency = selected.Efficiency
            };
        }

        /// <summary>
        /// Подобрать аккумулятор
        /// </summary>
        private Battery SelectBattery(double requiredCapacity, double voltage)
        {
            // Стандартные емкости аккумуляторов
            double[] standardCapacities = { 10, 13, 17, 20, 25, 30, 35, 40 };

            // Выбираем ближайшую большую стандартную емкость
            double selectedCapacity = standardCapacities.FirstOrDefault(c => c >= requiredCapacity);
            if (selectedCapacity == 0)
                selectedCapacity = standardCapacities.Last();

            // Максимальный ток из расчета 2C разряда
            double maxCurrent = selectedCapacity * 2;

            return new Battery(selectedCapacity, voltage)
            {
                MaxCurrent = maxCurrent
            };
        }

        /// <summary>
        /// Подобрать контроллер
        /// </summary>
        private Controller SelectController(Motor motor, Battery battery)
        {
            // Расчет требуемого тока контроллера
            double maxMotorCurrent = motor.MaxPower / motor.Voltage;

            // Стандартные токи контроллеров
            double[] standardCurrents = { 15, 18, 22, 25, 30, 35, 40, 45, 50 };

            // Выбираем контроллер с запасом 25%
            double requiredCurrent = maxMotorCurrent * 1.25;
            double selectedCurrent = standardCurrents.FirstOrDefault(c => c >= requiredCurrent);
            if (selectedCurrent == 0)
                selectedCurrent = standardCurrents.Last();

            return new Controller
            {
                MaxCurrent = selectedCurrent
            };
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Подобрать комплектующие на основе требований пользователя
        /// </summary>
        /// <param name="specs">Требования пользователя</param>
        /// <returns>Кортеж (мотор, батарея, контроллер)</returns>
        /// <exception cref="ArgumentException">Выбрасывается при некорректных параметрах</exception>
        public (Motor motor, Battery battery, Controller controller) SelectComponents(BikeSpecifications specs)
        {
            double requiredPower = CalculateRequiredPower(specs);
            double requiredCapacity = CalculateRequiredCapacity(specs, requiredPower);

            var motor = SelectMotor(requiredPower);
            var battery = SelectBattery(requiredCapacity, motor.Voltage);
            var controller = SelectController(motor, battery);

            return (motor, battery, controller);
        }

        /// <summary>
        /// Получить детальный отчет по подобранным компонентам
        /// </summary>
        public string GetComponentsReport(Motor motor, Battery battery, Controller controller)
        {
            double motorCurrent = motor.MaxPower / motor.Voltage;
            bool voltageMatch = Math.Abs(motor.Voltage - battery.NominalVoltage) < 0.1;
            bool controllerMatch = controller.MaxCurrent >= motorCurrent;
            bool batteryMatch = battery.MaxCurrent >= controller.MaxCurrent;

            return $@"=== ОТЧЕТ ПО ПОДОБРАННЫМ КОМПОНЕНТАМ ===

МОТОР:
• Номинальная мощность: {motor.Power:F0} Вт
• Максимальная мощность: {motor.MaxPower:F0} Вт
• Напряжение: {motor.Voltage:F0} В
• КПД: {motor.Efficiency:P0}

АККУМУЛЯТОР:
• Емкость: {battery.Capacity:F0} Ач ({battery.Capacity * battery.NominalVoltage:F0} Вт·ч)
• Напряжение: {battery.NominalVoltage:F0} В
• Максимальный ток: {battery.MaxCurrent:F0} А

КОНТРОЛЛЕР:
• Максимальный ток: {controller.MaxCurrent:F0} А

СОВМЕСТИМОСТЬ:
• Напряжение: {(voltageMatch ? "✓" : "✗")} {motor.Voltage}В / {battery.NominalVoltage}В
• Ток контроллера: {(controllerMatch ? "✓" : "⚠")} {controller.MaxCurrent}А / {motorCurrent:F0}А
• Ток батареи: {(batteryMatch ? "✓" : "⚠")} {battery.MaxCurrent}А / {controller.MaxCurrent}А";
        }

        /// <summary>
        /// Получить рекомендации по улучшению системы
        /// </summary>
        public System.Collections.Generic.List<string> GetRecommendations(Motor motor, Battery battery, Controller controller)
        {
            var recommendations = new List<string>();

            if (Math.Abs(motor.Voltage - battery.NominalVoltage) > 0.1)
                recommendations.Add("⚠ Напряжение мотора и аккумулятора не совпадает!");

            double motorMaxCurrent = motor.MaxPower / motor.Voltage;
            if (controller.MaxCurrent < motorMaxCurrent)
                recommendations.Add($"⚠ Контроллер ({controller.MaxCurrent}А) может не справиться с пиковым током мотора ({motorMaxCurrent:F1}А)");

            if (battery.MaxCurrent < controller.MaxCurrent)
                recommendations.Add($"⚠ Аккумулятор ({battery.MaxCurrent}А) может не отдать требуемый контроллером ток ({controller.MaxCurrent}А)");

            if (battery.Capacity * battery.NominalVoltage < 500)
                recommendations.Add("⚠ Малая емкость аккумулятора, пробег будет ограничен");

            if (motor.Power > 1000)
                recommendations.Add("⚠ Мощный мотор требует качественного охлаждения и прочной рамы");

            return recommendations;
        }

        #endregion
    }
}
