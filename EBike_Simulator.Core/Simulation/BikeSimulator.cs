using EBike_Simulator.Core.BikeComponents;
using EBike_Simulator.Core.Enums;
using EBike_Simulator.Core.Models;
using EBike_Simulator.Core.Models.Simulation;
using EBike_Simulator.Core.Services.WireService;
using EBike_Simulator.Core.Simulation.SimulationResults;
using EBike_Simulator.Core.Simulation.SimulationResults.Temperature;
using EBike_Simulator.Core.Simulation.SimulationResults.Wind;

namespace EBike_Simulator.Core.Simulation
{
    /// <summary>
    /// Основной класс симулятора движения электровелосипеда
    /// Моделирует физику движения, учитывая вес, аэродинамику, ветер, температуру и тепловые процессы
    /// </summary>
    public class BikeSimulator
    {
        #region Private Fields

        private readonly BikeSpecifications _specs;
        private readonly Motor _motor;
        private readonly Battery _battery;
        private readonly Controller _controller;
        private readonly Models.Environment _environment;
        private readonly WireSelector _wireSelector = new WireSelector();

        // Физические константы
        private const double RollingResistanceCoefficient = 0.01;  // Коэффициент сопротивления качению
        private const double Gravity = 9.81;                       // Ускорение свободного падения (м/с²)
        private const double AirDensity = 1.225;                   // Плотность воздуха (кг/м³)
        private const double DragCoefficient = 0.9;                // Коэффициент аэродинамического сопротивления
        private const double FrontalArea = 0.5;                    // Лобовая площадь (м²)

        #endregion

        #region Constructor

        /// <summary>
        /// Создает новый экземпляр симулятора
        /// </summary>
        /// <param name="specs">Параметры велосипеда и райдера</param>
        /// <param name="motor">Модель мотора</param>
        /// <param name="battery">Модель аккумулятора</param>
        /// <param name="controller">Модель контроллера</param>
        /// <param name="environment">Погодные условия</param>
        public BikeSimulator(
            BikeSpecifications specs,
            Motor motor,
            Battery battery,
            Controller controller,
            Models.Environment environment)
        {
            _specs = specs ?? throw new ArgumentNullException(nameof(specs));
            _motor = motor ?? throw new ArgumentNullException(nameof(motor));
            _battery = battery ?? throw new ArgumentNullException(nameof(battery));
            _controller = controller ?? throw new ArgumentNullException(nameof(controller));
            _environment = environment ?? throw new ArgumentNullException(nameof(environment));
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Рассчитать силу сопротивления качению
        /// </summary>
        private double CalculateRollingResistance()
        {
            return RollingResistanceCoefficient * _specs.TotalWeight * Gravity;
        }

        /// <summary>
        /// Рассчитать ускорение велосипеда
        /// </summary>
        private double CalculateAcceleration(double speedMs, double totalForce, double actualPower)
        {
            if (speedMs > 0.1)
            {
                double availableForce = actualPower / speedMs;
                return (availableForce - totalForce) / _specs.TotalWeight;
            }
            return 2.0;
        }

        /// <summary>
        /// Обновить скорость с учетом ускорения
        /// </summary>
        private double UpdateSpeed(double speedMs, double acceleration, double timeStep)
        {
            speedMs += acceleration * timeStep;
            return Math.Max(speedMs, 0);
        }

        /// <summary>
        /// Рассчитать влияние ветра на сопротивление в процентах
        /// </summary>
        private double CalculateWindEffect(double speedMs, Wind wind)
        {
            if (speedMs < 0.1) 
                return 0;

            double noWindResistance = 0.5 * AirDensity * DragCoefficient * FrontalArea * speedMs * speedMs;
            double withWindResistance = wind.GetEffectiveWindForce(speedMs * 3.6);
            double difference = withWindResistance - noWindResistance;

            if (noWindResistance <= 0) 
                return 0;

            double percent = (difference / noWindResistance) * 100;

            if (percent > 100) 
                percent = 100;

            if (percent < -50) 
                percent = -50;

            return percent;
        }

        /// <summary>
        /// Проверить необходимость остановки симуляции
        /// </summary>
        private bool ShouldStopSimulation(double acceleration, double speedMs, double time)
        {
            // Остановка при достижении равновесия сил
            if (Math.Abs(acceleration) < 0.01 && time > 5 && speedMs > 1.0)
                return true;

            // Остановка при нулевой скорости (застрял на подъеме)
            if (speedMs < 0.1 && time > 10)
                return true;

            return false;
        }

        /// <summary>
        /// Найти положение дросселя для поддержания заданной скорости
        /// </summary>
        private double FindThrottleForSpeed(double targetSpeed)
        {
            double bestThrottle = 0.5;
            double bestSpeed = 0;
            double minDifference = double.MaxValue;

            // Сначала проверим максимальную скорость
            var maxTest = Simulate(1.0, 60, 0.1);
            double maxSpeed = maxTest.MaxSpeed;

            // Если запрошенная скорость выше максимальной, возвращаем 1.0
            if (targetSpeed > maxSpeed)
            {
                return 1.0;
            }

            // Перебираем throttle от 0.1 до 1.0
            for (double t = 0.1; t <= 1.0; t += 0.05)
            {
                var test = Simulate(t, 30, 0.1);
                if (test.Data.Count == 0) continue;

                double speed = test.Data.Last().Speed;
                double difference = Math.Abs(speed - targetSpeed);

                if (difference < minDifference)
                {
                    minDifference = difference;
                    bestSpeed = speed;
                    bestThrottle = t;
                }

                // Если нашли достаточно близко, выходим
                if (difference < 1.0)
                {
                    break;
                }
            }

            return bestThrottle;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Основной метод симуляции движения
        /// </summary>
        /// <param name="throttle">Положение ручки газа (0.0-1.0)</param>
        /// <param name="maxTime">Максимальное время симуляции в секундах</param>
        /// <param name="timeStep">Шаг времени в секундах (по умолчанию 0.1с)</param>
        /// <returns>Результаты симуляции</returns>
        public SimulationResult Simulate(double throttle, double maxTime, double timeStep = 0.1)
        {
            var result = new SimulationResult();

            // Сброс состояния компонентов
            _motor.Reset(_environment.Temperature);
            _controller.Reset();
            _battery.Reset();

            double speed = 0;        // км/ч
            double distance = 0;     // км
            double time = 0;

            double totalWindEffect = 0;
            double totalTempEffect = 0;
            int samples = 0;

            while (time < maxTime && _battery.HasCharge())
            {
                double speedMs = speed / 3.6; //  в м/с

                // 1. Расчет сил сопротивления
                double rollingForce = CalculateRollingResistance();
                double windForce = _environment.Wind.GetEffectiveWindForce(speed);
                double totalForce = rollingForce + windForce;

                // 2. Мощность от мотора
                double motorPower = _motor.GetOutputPower(throttle);

                // 3. Доступная сила от мотора (с защитой от деления на 0)
                double availableForce;
                if (speedMs > 0.1)
                {
                    availableForce = motorPower / speedMs;
                }
                else
                {
                    availableForce = motorPower / 0.1; // При очень малой скорости
                }

                // 4. Результирующая сила
                double netForce = availableForce - totalForce;

                // 5. Ускорение
                double acceleration = netForce / _specs.TotalWeight;

                // 6. Ограничиваем ускорение разумными пределами
                acceleration = Math.Clamp(acceleration, -5.0, 5.0);

                // 7. Если стоим на месте и сил не хватает - не даем отрицательного ускорения
                if (speedMs < 0.01 && netForce < 0)
                {
                    acceleration = 0;
                }

                // 8. Обновляем скорость
                speedMs += acceleration * timeStep;
                if (speedMs < 0) speedMs = 0;
                speed = speedMs * 3.6;
                distance += speedMs * timeStep / 1000;

                // 9. Рассчитываем реальную мощность на колесах
                double actualPower;
                if (speedMs > 0.01)
                {
                    actualPower = (availableForce - netForce) * speedMs;
                    if (actualPower < 0) actualPower = 0;
                }
                else
                {
                    actualPower = motorPower;
                }

                // 10. Расчет тока
                double requiredCurrent = _motor.CalculateRequiredCurrent(actualPower);
                double maxBatteryCurrent = _battery.GetMaxDischargeCurrent();
                double actualCurrent = Math.Min(requiredCurrent, maxBatteryCurrent);
                actualCurrent = _controller.GetOutputCurrent(throttle, _battery.Voltage);

                // 11. Расход энергии
                _battery.Use(actualCurrent, timeStep / 3600, _environment.Temperature);

                // 12. Обновление температур
                _motor.UpdateTemp(actualPower, _environment.Temperature, timeStep);
                _controller.UpdateTemp(actualCurrent, _environment.Temperature, timeStep);

                // 13. Расчет влияния внешних факторов
                double windEffect = CalculateWindEffect(speedMs, _environment.Wind);
                double tempEffect = _battery.GetTemperatureImpactOnRange();

                totalWindEffect += windEffect;
                totalTempEffect += tempEffect;
                samples++;

                // 14. Сохранение точки данных
                result.Data.Add(new SimulationData
                {
                    Time = time,
                    Speed = speed,
                    Distance = distance,
                    MotorTemp = _motor.Temperature,
                    ControllerTemp = _controller.Temperature,
                    BatteryTemp = _battery.Temperature,
                    BatterySOC = _battery.SOC,
                    Current = actualCurrent,
                    Power = actualPower,
                    WindEffect = windEffect,
                    TempEffect = tempEffect
                });

                time += timeStep;

                // 15. Проверка условий остановки
                if (ShouldStopSimulation(acceleration, speedMs, time))
                    break;
            }

            // 16. Заполнение итоговых результатов
            result.TotalTime = time;
            result.TotalDistance = distance;
            result.MaxSpeed = result.Data.Count > 0 ? result.Data.Max(d => d.Speed) : 0;
            result.BatteryEmpty = !_battery.HasCharge();
            result.AverageWindImpact = samples > 0 ? totalWindEffect / samples : 0;
            result.AverageTempImpact = samples > 0 ? totalTempEffect / samples : 0;
            result.FinalBatteryTemp = _battery.Temperature;

            return result;
        }

        /// <summary>
        /// Тест разгона с полным газом
        /// </summary>
        /// <param name="targetSpeed">Целевая скорость (по умолчанию берется из спецификаций)</param>
        /// <returns>Результаты симуляции разгона</returns>
        public SimulationResult TestAcceleration(double targetSpeed = 0)
        {
            if (targetSpeed == 0) targetSpeed = _specs.DesiredMaxSpeed;
            return Simulate(1.0, 30, 0.05);
        }

        /// <summary>
        /// Тест пробега на постоянной скорости
        /// </summary>
        /// <param name="constantSpeed">Целевая постоянная скорость (км/ч)</param>
        /// <returns>Результаты симуляции пробега</returns>
        public SimulationResult TestRange(double constantSpeed)
        {
            var maxTest = Simulate(1.0, 30, 0.1);
            double maxPossibleSpeed = maxTest.MaxSpeed;

            double targetSpeed = constantSpeed;
            if (constantSpeed > maxPossibleSpeed)
            {
                targetSpeed = maxPossibleSpeed * 0.9;
            }

            double throttle = FindThrottleForSpeed(targetSpeed);

            // ИСПОЛЬЗУЕМ НОВЫЙ МЕТОД!
            var result = SimulateRange(throttle, 3600 * 5, 1.0);

            return result;
        }

        public SimulationResult SimulateRange(double throttle, double maxTime, double timeStep = 1.0)
        {
            var result = new SimulationResult();

            _motor.Reset(_environment.Temperature);
            _controller.Reset();
            _battery.Reset();

            double speed = 0;
            double distance = 0;
            double time = 0;

            while (time < maxTime && _battery.HasCharge())
            {
                double speedMs = speed / 3.6;

                double rollingForce = CalculateRollingResistance();
                double windForce = _environment.Wind.GetEffectiveWindForce(speed);
                double totalForce = rollingForce + windForce;

                double motorPower = _motor.GetOutputPower(throttle);

                double availableForce;
                if (speedMs > 0.1)
                    availableForce = motorPower / speedMs;
                else
                    availableForce = motorPower / 0.1;

                double netForce = availableForce - totalForce;
                double acceleration = netForce / _specs.TotalWeight;
                acceleration = Math.Clamp(acceleration, -5.0, 5.0);

                if (speedMs < 0.01 && netForce < 0)
                    acceleration = 0;

                speedMs += acceleration * timeStep;
                if (speedMs < 0) speedMs = 0;
                speed = speedMs * 3.6;
                distance += speedMs * timeStep / 1000;

                double actualPower;
                if (speedMs > 0.01)
                {
                    actualPower = (availableForce - netForce) * speedMs;
                    if (actualPower < 0) actualPower = 0;
                }
                else
                {
                    actualPower = motorPower;
                }

                double requiredCurrent = _motor.CalculateRequiredCurrent(actualPower);
                double maxBatteryCurrent = _battery.GetMaxDischargeCurrent();
                double actualCurrent = Math.Min(requiredCurrent, maxBatteryCurrent);
                actualCurrent = _controller.GetOutputCurrent(throttle, _battery.Voltage);

                _battery.Use(actualCurrent, timeStep / 3600, _environment.Temperature);

                result.Data.Add(new SimulationData
                {
                    Time = time,
                    Speed = speed,
                    Distance = distance,
                    MotorTemp = _motor.Temperature,
                    ControllerTemp = _controller.Temperature,
                    BatteryTemp = _battery.Temperature,
                    BatterySOC = _battery.SOC,
                    Current = actualCurrent,
                    Power = actualPower,
                    WindEffect = 0,
                    TempEffect = 0
                });

                time += timeStep;

                if (!_battery.HasCharge())
                    break;
            }

            result.TotalTime = time;
            result.TotalDistance = distance;
            result.MaxSpeed = result.Data.Count > 0 ? result.Data.Max(d => d.Speed) : 0;
            result.BatteryEmpty = !_battery.HasCharge();

            return result;
        }

        /// <summary>
        /// Анализ проводки на основе результатов симуляции
        /// </summary>
        /// <param name="result">Результаты симуляции</param>
        /// <param name="batteryToControllerLength">Длина провода от батареи к контроллеру (м)</param>
        /// <param name="controllerToMotorLength">Длина провода от контроллера к мотору (м)</param>
        /// <returns>Результаты анализа проводки с рекомендациями</returns>
        public WiringAnalysis AnalyzeWiring(
            SimulationResult result,
            double batteryToControllerLength = 0.5,
            double controllerToMotorLength = 1.0)
        {
            var analysis = new WiringAnalysis();

            if (result.Data.Count == 0)
                return analysis;

            double maxBatteryCurrent = result.Data.Max(d => d.Current);
            double maxMotorCurrent = maxBatteryCurrent * 1.2; 

            var wires = _wireSelector.SelectWiring(
                maxBatteryCurrent,
                maxMotorCurrent,
                batteryToControllerLength,
                controllerToMotorLength,
                _battery.NominalVoltage
            );

            analysis.RecommendedWires = wires;
            analysis.MaxBatteryCurrent = maxBatteryCurrent;
            analysis.MaxMotorCurrent = maxMotorCurrent;

            if (result.Data.Count > 0)
            {
                double avgCurrent = result.Data.Average(d => d.Current);
                var batteryWire = wires["Battery_to_Controller"];
                analysis.EstimatedAvgPowerLoss =
                    batteryWire.CalculatePowerLoss(avgCurrent, batteryToControllerLength);
            }

            return analysis;
        }

        /// <summary>
        /// Тестирование влияния температуры на пробег
        /// </summary>
        /// <param name="speed">Скорость теста (км/ч)</param>
        /// <returns>Результаты анализа температурного влияния</returns>
        public TemperatureImpact TestTemperatureImpact(double speed)
        {
            var impact = new TemperatureImpact();

            double[] testTemperatures = { -10, 0, 10, 20, 30, 40 };

            foreach (double temp in testTemperatures)
            {
                var env = new Models.Environment { Temperature = temp };

                var testBattery = _battery.Clone();
                testBattery.ForceTemperature(temp); 

                var tempSimulator = new BikeSimulator(
                    _specs.Clone(),
                    _motor.Clone(),
                    testBattery,
                    _controller.Clone(),
                    env
                );

                var result = tempSimulator.TestRange(speed);
                impact.AddTest(temp, result.TotalDistance, result.TotalTime, result.FinalBatteryTemp);
            }

            return impact;
        }

        /// <summary>
        /// Тестирование влияния ветра на пробег
        /// </summary>
        /// <param name="speed">Скорость теста (км/ч)</param>
        /// <returns>Результаты анализа влияния ветра</returns>
        public WindImpactTest TestWindImpact(double speed)
        {
            var test = new WindImpactTest();

            WindDirection[] directions = { WindDirection.Headwind, WindDirection.Tailwind, WindDirection.Crosswind };
            double[] windSpeeds = { 0, 5, 10, 15 };

            foreach (var direction in directions)
            {
                foreach (var windSpeed in windSpeeds)
                {
                    var env = new Models.Environment
                    {
                        Temperature = _environment.Temperature,
                        Wind = new Wind { Speed = windSpeed, Direction = direction }
                    };

                    var windSimulator = new BikeSimulator(
                        _specs.Clone(),
                        _motor.Clone(),
                        _battery.Clone(),
                        _controller.Clone(),
                        env
                    );

                    var result = windSimulator.TestRange(speed);

                    test.AddResult(direction, windSpeed, result.TotalDistance, result.TotalTime);
                }
            }

            return test;
        }

        #endregion
    }
}
