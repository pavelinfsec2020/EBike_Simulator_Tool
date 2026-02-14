using EBike_Simulator.Core.BikeComponents;
using EBike_Simulator.Core.Models;

namespace EBike_Simulator.Core.Services
{
    /// <summary>
    /// Сервис для автоматического подбора компонентов электровелосипеда
    /// с учетом веса компонентов
    /// </summary>
    public class ComponentSelector
    {
        #region Private Methods

        /// <summary>
        /// Рассчитать требуемую мощность для достижения желаемой скорости
        /// </summary>
        private double CalculateRequiredPower(BikeSpecifications specs, double additionalWeight)
        {
            double totalWeight = specs.TotalWeight + additionalWeight;
            double speedMs = specs.DesiredMaxSpeed / 3.6;
            
            double rollingResistance = 0.01 * totalWeight * 9.81;
            double airResistance = 0.5 * 1.225 * 0.9 * 0.5 * speedMs * speedMs;
            double totalForce = rollingResistance + airResistance;
            
            double powerAtWheels = totalForce * speedMs;
            double requiredMotorPower = powerAtWheels / 0.85 * 1.2;
            
            return requiredMotorPower;
        }

        /// <summary>
        /// Определить оптимальное напряжение системы на основе требуемой мощности
        /// </summary>
        private double DetermineSystemVoltage(double requiredPower)
        {
            
            if (requiredPower <= 500)
                return 36.0;
            else if (requiredPower <= 1500)
                return 48.0;
            else if (requiredPower <= 3000)
                return 62.0;
            else
                return 72.0;
        }

        /// <summary>
        /// Рассчитать требуемую емкость аккумулятора
        /// </summary>
        private double CalculateRequiredCapacity(BikeSpecifications specs, double motorPower, double systemVoltage, double additionalWeight)
        {
            double totalWeight = specs.TotalWeight + additionalWeight;
            double averageSpeedKmh = specs.DesiredMaxSpeed * 0.8;
            double averageSpeedMs = averageSpeedKmh / 3.6;
            
            double rollingResistance = 0.01 * totalWeight * 9.81;
            double airResistance = 0.5 * 1.225 * 0.9 * 0.5 * averageSpeedMs * averageSpeedMs;
            double totalForce = rollingResistance + airResistance;
            
            double averagePower = totalForce * averageSpeedMs / 0.85;
            double operatingHours = specs.DesiredMaxRange / averageSpeedKmh;
            double requiredEnergy = averagePower * operatingHours;
            double requiredCapacity = requiredEnergy / systemVoltage;
            
            return requiredCapacity * 1.2; 
        }

        /// <summary>
        /// Получить примерный вес мотора по мощности
        /// </summary>
        private double GetMotorWeight(double motorPower)
        {
            if (motorPower <= 500)
                return 2.5;
            else if (motorPower <= 1000)
                return 3.5;
            else if (motorPower <= 2000)
                return 5.0;
            else if (motorPower <= 3000)
                return 7.0;
            else
                return 9.0;
        }

        /// <summary>
        /// Получить примерный вес батареи по емкости и напряжению
        /// </summary>
        private double GetBatteryWeight(double capacity, double voltage)
        {
            // Li-ion ячейки: ~0.05 кг на Вт·ч
            double energyWh = capacity * voltage;
            double weightKg = energyWh / 200.0; 

            return weightKg;
        }

        /// <summary>
        /// Подобрать мотор из доступных вариантов
        /// </summary>
        private Motor SelectMotor(double requiredPower, double systemVoltage)
        {
            var availableMotors = new[]
            {
                // 36V 
                new { Name = "Bafang BBS01B", Voltage = 36.0, Power = 250.0, MaxPower = 350.0, Efficiency = 0.82, Weight = 2.8 },
                new { Name = "Tongsheng TSDZ2-36", Voltage = 36.0, Power = 350.0, MaxPower = 500.0, Efficiency = 0.83, Weight = 3.2 },
                
                // 48V 
                new { Name = "Bafang BBS02B", Voltage = 48.0, Power = 500.0, MaxPower = 750.0, Efficiency = 0.85, Weight = 3.5 },
                new { Name = "Bafang BBSHD", Voltage = 48.0, Power = 1000.0, MaxPower = 1500.0, Efficiency = 0.87, Weight = 4.5 },
                new { Name = "Tongsheng TSDZ2-48", Voltage = 48.0, Power = 750.0, MaxPower = 1000.0, Efficiency = 0.85, Weight = 3.8 },
                
                // 62V 
                new { Name = "CYC X1 Pro 62V", Voltage = 62.0, Power = 2000.0, MaxPower = 3000.0, Efficiency = 0.88, Weight = 5.5 },
                new { Name = "Bafang Ultra 62V", Voltage = 62.0, Power = 1500.0, MaxPower = 2500.0, Efficiency = 0.87, Weight = 5.0 },
                new { Name = "QS Motor 138", Voltage = 62.0, Power = 2000.0, MaxPower = 3500.0, Efficiency = 0.88, Weight = 6.5 },
                
                // 72V 
                new { Name = "QS Motor 205", Voltage = 72.0, Power = 3000.0, MaxPower = 5000.0, Efficiency = 0.89, Weight = 8.5 },
                new { Name = "MXUS 3000W", Voltage = 72.0, Power = 3000.0, MaxPower = 4500.0, Efficiency = 0.88, Weight = 7.5 },
                new { Name = "CYC X1 Stealth", Voltage = 72.0, Power = 3000.0, MaxPower = 6000.0, Efficiency = 0.89, Weight = 6.8 },
                new { Name = "QS Motor 205 8kW", Voltage = 72.0, Power = 5000.0, MaxPower = 8000.0, Efficiency = 0.90, Weight = 12.5 },
            };

            var compatibleMotors = availableMotors
                .Where(m => Math.Abs(m.Voltage - systemVoltage) < 0.5)
                .OrderBy(m => m.Power)
                .ToList();

            if (!compatibleMotors.Any())
            {
                compatibleMotors = availableMotors
                    .OrderBy(m => Math.Abs(m.Voltage - systemVoltage))
                    .ThenBy(m => m.Power)
                    .ToList();
            }

            double requiredWithMargin = requiredPower * 1.2;
            
            var selected = compatibleMotors
                .Where(m => m.Power >= requiredWithMargin)
                .FirstOrDefault();
            
            if (selected == null && compatibleMotors.Any())
            {
                selected = compatibleMotors.OrderByDescending(m => m.Power).First();
            }
            
            if (selected == null)
            {
                selected = availableMotors.OrderByDescending(m => m.Power).First();
            }

            return new Motor
            {
                Name = selected.Name,
                Voltage = selected.Voltage,
                Power = selected.Power,
                MaxPower = selected.MaxPower,
                Efficiency = selected.Efficiency,
                Weight = selected.Weight
            };
        }

        /// <summary>
        /// Подобрать аккумулятор
        /// </summary>
        private Battery SelectBattery(double requiredCapacity, double systemVoltage)
        {
            int[] standardCapacities;
            
            if (systemVoltage <= 36)
                standardCapacities = new[] { 8, 10, 12, 15, 17, 20 };
            else if (systemVoltage <= 48)
                standardCapacities = new[] { 10, 13, 15, 17, 20, 25, 30 };
            else if (systemVoltage <= 62)
                standardCapacities = new[] { 12, 15, 18, 20, 25, 30, 35, 40 };
            else 
                standardCapacities = new[] { 15, 20, 25, 30, 35, 40, 45, 50, 60, 70, 80, 90, 100 };

            double selectedCapacity = standardCapacities.FirstOrDefault(c => c >= requiredCapacity);
            if (selectedCapacity == 0)
                selectedCapacity = standardCapacities.Last();

            double maxDischargeCurrent = selectedCapacity * 2.0;
            
            if (systemVoltage <= 36)
                maxDischargeCurrent = Math.Min(maxDischargeCurrent, 40.0);
            else if (systemVoltage <= 48)
                maxDischargeCurrent = Math.Min(maxDischargeCurrent, 50.0);
            else if (systemVoltage <= 62)
                maxDischargeCurrent = Math.Min(maxDischargeCurrent, 70.0);
            else
                maxDischargeCurrent = Math.Min(maxDischargeCurrent, 100.0);

            double batteryWeight = GetBatteryWeight(selectedCapacity, systemVoltage);

            return new Battery(selectedCapacity, systemVoltage)
            {
                Name = $"{systemVoltage}V {selectedCapacity}Ah Li-ion",
                MaxCurrent = maxDischargeCurrent,
                Weight = batteryWeight
            };
        }

        /// <summary>
        /// Подобрать контроллер
        /// </summary>
        private Controller SelectController(Motor motor, Battery battery)
        {
            double motorMaxCurrent = motor.MaxPower / motor.Voltage; 
            double requiredControllerCurrent = motorMaxCurrent * 1.15; 

            int[] standardCurrents;

            if (motor.Voltage <= 36)
                standardCurrents = new[] { 15, 20, 25, 30, 35, 40 };
            else if (motor.Voltage <= 48)
                standardCurrents = new[] { 20, 25, 30, 35, 40, 45, 50 }; 
            else if (motor.Voltage <= 62)
                standardCurrents = new[] { 30, 35, 40, 45, 50, 60, 70, 80 };
            else
                standardCurrents = new[] { 40, 50, 60, 70, 80, 90, 100, 120, 150, 200, 250, 300, 350 };

            double selectedCurrent = standardCurrents.FirstOrDefault(c => c >= requiredControllerCurrent);
            
            if (selectedCurrent == 0)
                selectedCurrent = standardCurrents.Last();

            selectedCurrent = Math.Min(selectedCurrent, battery.MaxCurrent);
            selectedCurrent = standardCurrents.OrderBy(c => Math.Abs(c - selectedCurrent)).First();

            double controllerWeight = selectedCurrent switch
            {
                <= 30 => 0.6,
                <= 40 => 0.8,  
                <= 50 => 1.0,
                <= 70 => 1.2,
                _ => 1.5
            };

            return new Controller
            {
                Name = $"{motor.Voltage}V {selectedCurrent}A Controller",
                MaxCurrent = selectedCurrent,
                Weight = controllerWeight
            };
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Подобрать комплектующие с учетом веса
        /// </summary>
        public (Motor motor, Battery battery, Controller controller) SelectComponents(BikeSpecifications specs)
        {  
            double estimatedComponentsWeight = 8.0; 
            
            Motor motor = null;
            Battery battery = null;
            Controller controller = null;
            
            for (int iteration = 0; iteration < 3; iteration++)
            {
                double requiredPower = CalculateRequiredPower(specs, estimatedComponentsWeight);
                double systemVoltage = DetermineSystemVoltage(requiredPower);
                
                motor = SelectMotor(requiredPower, systemVoltage);
                
                double requiredCapacity = CalculateRequiredCapacity(specs, motor.Power, systemVoltage, estimatedComponentsWeight);
                battery = SelectBattery(requiredCapacity, systemVoltage);
                
                controller = SelectController(motor, battery);              
                estimatedComponentsWeight = motor.Weight + battery.Weight + controller.Weight;
            }

            Console.WriteLine($"\n[DEBUG] Расчетные параметры:");
            Console.WriteLine($"[DEBUG] Вес компонентов: {motor.Weight + battery.Weight + controller.Weight:F1} кг");
            Console.WriteLine($"[DEBUG] Общий вес системы: {specs.TotalWeight + motor.Weight + battery.Weight + controller.Weight:F1} кг");
            Console.WriteLine($"[DEBUG] Выбрано напряжение: {motor.Voltage:F0} В");

            return (motor, battery, controller);
        }

        /// <summary>
        /// Получить детальный отчет по подобранным компонентам
        /// </summary>
        public string GetComponentsReport(BikeSpecifications specs, Motor motor, Battery battery, Controller controller)
        {
            double motorMaxCurrent = motor.MaxPower / motor.Voltage;
            bool voltageMatch = Math.Abs(motor.Voltage - battery.NominalVoltage) < 0.5;
            bool controllerMatch = controller.MaxCurrent >= motorMaxCurrent;
            bool batteryMatch = battery.MaxCurrent >= controller.MaxCurrent;

            double totalWeight = specs.TotalWeight + motor.Weight + battery.Weight + controller.Weight;

            return $@"
=== ОТЧЕТ ПО ПОДОБРАННЫМ КОМПОНЕНТАМ ===

МОТОР:
• Модель: {motor.Name}
• Номинальная мощность: {motor.Power:F0} Вт
• Максимальная мощность: {motor.MaxPower:F0} Вт
• Напряжение: {motor.Voltage:F0} В
• КПД: {motor.Efficiency:P0}
• Вес: {motor.Weight:F1} кг

АККУМУЛЯТОР:
• Модель: {battery.Name}
• Емкость: {battery.Capacity:F0} Ач ({battery.Capacity * battery.NominalVoltage:F0} Вт·ч)
• Напряжение: {battery.NominalVoltage:F0} В
• Максимальный ток: {battery.MaxCurrent:F0} А
• Вес: {battery.Weight:F1} кг

КОНТРОЛЛЕР:
• Модель: {controller.Name}
• Максимальный ток: {controller.MaxCurrent:F0} А
• Вес: {controller.Weight:F1} кг

ВЕС СИСТЕМЫ:
• Велосипед: {specs.BikeWeight:F1} кг
• Велосипедист: {specs.RiderWeight:F1} кг
• Компоненты: {motor.Weight + battery.Weight + controller.Weight:F1} кг
• ОБЩИЙ ВЕС: {totalWeight:F1} кг

СОВМЕСТИМОСТЬ:
• Напряжение: {(voltageMatch ? "✓" : "⚠")} {motor.Voltage}В / {battery.NominalVoltage}В
• Ток контроллера: {(controllerMatch ? "✓" : "⚠")} {controller.MaxCurrent}А / {motorMaxCurrent:F0}А
• Ток батареи: {(batteryMatch ? "✓" : "⚠")} {battery.MaxCurrent}А / {controller.MaxCurrent}А";
        }

        /// <summary>
        /// Получить рекомендации по улучшению системы
        /// </summary>
        public System.Collections.Generic.List<string> GetRecommendations(Motor motor, Battery battery, Controller controller)
        {
            var recommendations = new System.Collections.Generic.List<string>();

            double motorMaxCurrent = motor.MaxPower / motor.Voltage;
            
            if (Math.Abs(motor.Voltage - battery.NominalVoltage) > 0.5)
                recommendations.Add($"⚠ Напряжение мотора ({motor.Voltage}В) и батареи ({battery.NominalVoltage}В) не совпадают!");
            
            if (controller.MaxCurrent < motorMaxCurrent)
                recommendations.Add($"⚠ Контроллер ({controller.MaxCurrent}А) может не справиться с пиковым током мотора ({motorMaxCurrent:F1}А)");

            if (battery.MaxCurrent < controller.MaxCurrent - 2)
                recommendations.Add($"⚠ Аккумулятор ({battery.MaxCurrent}А) может не отдать требуемый контроллером ток ({controller.MaxCurrent}А)");

            if (battery.Capacity * battery.NominalVoltage < 500)
                recommendations.Add("⚠ Малая емкость аккумулятора, пробег будет ограничен");

            if (motor.Power > 1500)
                recommendations.Add("⚠ Мощный мотор требует качественного охлаждения и усиленной рамы");

            return recommendations;
        }

        #endregion     
    }
}
