using EBike_Simulator.Core;
using EBike_Simulator.Core.BikeComponents;
using EBike_Simulator.Core.Enums;
using EBike_Simulator.Core.Models;
using EBike_Simulator.Core.Models.Simulation;
using EBike_Simulator.Core.Services;
using EBike_Simulator.Core.Services.WireService;
using EBike_Simulator.Core.Simulation;
using EBike_Simulator.Data;
using EBike_Simulator.Data.Models;
using System.Runtime.InteropServices;
using Environment = EBike_Simulator.Core.Models.Environment;

namespace ElectricBikeSimulation.ConsoleTest
{
    /// <summary>
    /// Главный класс консольного приложения
    /// </summary>
    /// <summary>
    /// Главный класс консольного приложения
    /// </summary>
    public class Program
    {
        #region Static Fields

        private static BikeSpecifications _currentSpecs;
        private static Environment _currentEnvironment;
        private static Motor _currentMotor;
        private static Battery _currentBattery;
        private static Controller _currentController;
        private static BikeSimulator _currentSimulator;
        private static SimulationResult _lastTestResult;
        private static readonly ComponentSelector _componentSelector = new ComponentSelector();

        #endregion

        #region Main Entry Point

        static  void Main(string[] args)
        {
            
            //data test
            var dataInit = new DataInitializer();
            dataInit.StartAsync();
            var init = new Initializer(dataInit.TranslationRepos);
            init.Language = Language.En;
            Console.WriteLine(init.Translater.Translate("warm", init.Language).Result);
            init.Language = Language.Ru;
            Console.WriteLine(init.Translater.Translate("warm", init.Language).Result);

            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.WriteLine("╔══════════════════════════════════════════════════════════╗");
            Console.WriteLine("║         СИМУЛЯТОР ЭЛЕКТРОВЕЛОСИПЕДА v2.0                 ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════╝");

            InitializeApplication();

            bool exitRequested = false;
            while (!exitRequested)
            {
                exitRequested = ShowMainMenu();
            }

            Console.WriteLine("\nБлагодарим за использование симулятора!");
            Console.WriteLine("Нажмите любую клавишу для выхода...");
            Console.ReadKey();
        }

        #endregion

        #region Initialization

        private static void InitializeApplication()
        {
            Console.WriteLine("\n▶ Инициализация системы...");

            _currentSpecs = new BikeSpecifications
            {
                RiderWeight = 80,
                BikeWeight = 25,
                WheelDiameter = 26,
                DesiredMaxSpeed = 35,
                DesiredMaxRange = 50
            };

            _currentEnvironment = new Environment
            {
                Temperature = 20.0,
                Wind = new Wind { Speed = 0, Direction = WindDirection.Headwind }
            };

            Console.WriteLine("✓ Система инициализирована с настройками по умолчанию.");
            Console.WriteLine("  Выполните подбор комплектующих для начала работы.");
        }

        #endregion

        #region Menu System

        private static bool ShowMainMenu()
        {
            Console.WriteLine("\n╔══════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                      ГЛАВНОЕ МЕНЮ                        ║");
            Console.WriteLine("╠══════════════════════════════════════════════════════════╣");
            Console.WriteLine("║  1. Настройка параметров велосипеда                      ║");
            Console.WriteLine("║  2. Настройка погодных условий                           ║");
            Console.WriteLine("║  3. ПОДБОР КОМПЛЕКТУЮЩИХ (с учетом веса)                  ║");
            Console.WriteLine("║  4. Тест разгона                                          ║");
            Console.WriteLine("║  5. Тест пробега                                          ║");
            Console.WriteLine("║  6. Анализ проводки                                       ║");
            Console.WriteLine("║  7. Анализ влияния температуры                            ║");
            Console.WriteLine("║  8. Анализ влияния ветра                                  ║");
            Console.WriteLine("║  9. Экспорт данных                                        ║");
            Console.WriteLine("║ 10. Показать текущую конфигурацию                         ║");
            Console.WriteLine("║  0. Выход                                                  ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════╝");

            Console.Write("\n▶ Выберите действие: ");
            string choice = Console.ReadLine();

            switch (choice)
            {
                case "1": ConfigureBikeSpecifications(); break;
                case "2": ConfigureEnvironment(); break;
                case "3": SelectComponents(); break;
                case "4": TestAcceleration(); break;
                case "5": TestRange(); break;
                case "6": AnalyzeWiring(); break;
                case "7": AnalyzeTemperatureImpact(); break;
                case "8": AnalyzeWindImpact(); break;
                case "9": ExportData(); break;
                case "10": ShowCurrentConfiguration(); break;
                case "0": return true;
                default:
                    Console.WriteLine("❌ Неверный выбор. Попробуйте снова.");
                    break;
            }

            Console.WriteLine("\n▶ Нажмите любую клавишу для продолжения...");
            Console.ReadKey();
            return false;
        }

        #endregion

        #region Configuration Methods

        private static void ConfigureBikeSpecifications()
        {
            Console.WriteLine("\n╔══════════════════════════════════════════════════════════╗");
            Console.WriteLine("║              НАСТРОЙКА ПАРАМЕТРОВ ВЕЛОСИПЕДА             ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════╝");

            Console.Write($"Вес велосипедиста (кг) [{_currentSpecs.RiderWeight}]: ");
            string input = Console.ReadLine();
            if (!string.IsNullOrEmpty(input) && double.TryParse(input, out double riderWeight))
                _currentSpecs.RiderWeight = riderWeight;

            Console.Write($"Вес велосипеда (кг) [{_currentSpecs.BikeWeight}]: ");
            input = Console.ReadLine();
            if (!string.IsNullOrEmpty(input) && double.TryParse(input, out double bikeWeight))
                _currentSpecs.BikeWeight = bikeWeight;

            Console.Write($"Диаметр колес (дюймы) [{_currentSpecs.WheelDiameter}]: ");
            input = Console.ReadLine();
            if (!string.IsNullOrEmpty(input) && double.TryParse(input, out double wheelDiameter))
                _currentSpecs.WheelDiameter = wheelDiameter;

            Console.Write($"Желаемая максимальная скорость (км/ч) [{_currentSpecs.DesiredMaxSpeed}]: ");
            input = Console.ReadLine();
            if (!string.IsNullOrEmpty(input) && double.TryParse(input, out double maxSpeed))
                _currentSpecs.DesiredMaxSpeed = maxSpeed;

            Console.Write($"Желаемый пробег (км) [{_currentSpecs.DesiredMaxRange}]: ");
            input = Console.ReadLine();
            if (!string.IsNullOrEmpty(input) && double.TryParse(input, out double maxRange))
                _currentSpecs.DesiredMaxRange = maxRange;
        }

        private static void ConfigureEnvironment()
        {
            Console.WriteLine("\n╔══════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                НАСТРОЙКА ПОГОДНЫХ УСЛОВИЙ                 ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════╝");

            Console.Write($"Температура окружающей среды (°C) [{_currentEnvironment.Temperature}]: ");
            string input = Console.ReadLine();
            if (!string.IsNullOrEmpty(input) && double.TryParse(input, out double temp))
                _currentEnvironment.Temperature = temp;

            Console.WriteLine($"\nТекущая температура: {_currentEnvironment.Temperature}°C");

            Console.WriteLine("\nНастройка ветра:");
            Console.WriteLine("  1. Без ветра");
            Console.WriteLine("  2. Легкий ветер (5 м/с)");
            Console.WriteLine("  3. Средний ветер (10 м/с)");
            Console.WriteLine("  4. Сильный ветер (15 м/с)");
            Console.Write($"Выберите силу ветра [1]: ");

            string windChoice = Console.ReadLine();
            if (!string.IsNullOrEmpty(windChoice))
            {
                double windSpeed = windChoice switch
                {
                    "1" => 0,
                    "2" => 5,
                    "3" => 10,
                    "4" => 15,
                    _ => _currentEnvironment.Wind.Speed
                };
                _currentEnvironment.Wind.Speed = windSpeed;
            }

            if (_currentEnvironment.Wind.Speed > 0)
            {
                Console.WriteLine("\nНаправление ветра:");
                Console.WriteLine("  1. Встречный (против движения)");
                Console.WriteLine("  2. Попутный (по движению)");
                Console.WriteLine("  3. Боковой");
                Console.Write($"Выберите направление [1]: ");

                string dirChoice = Console.ReadLine();
                if (!string.IsNullOrEmpty(dirChoice))
                {
                    _currentEnvironment.Wind.Direction = dirChoice switch
                    {
                        "2" => WindDirection.Tailwind,
                        "3" => WindDirection.Crosswind,
                        _ => WindDirection.Headwind
                    };
                }
            }

            Console.WriteLine($"\n✓ Установлены условия: {_currentEnvironment.Temperature}°C, " +
                            $"ветер {_currentEnvironment.Wind.Speed} м/с ({_currentEnvironment.Wind.GetDirectionName()})");
        }

        private static void SelectComponents()
        {
            Console.WriteLine("\n╔══════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                 ПОДБОР КОМПЛЕКТУЮЩИХ                     ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════╝");

            try
            {
                (_currentMotor, _currentBattery, _currentController) =
                    _componentSelector.SelectComponents(_currentSpecs);

                Console.WriteLine(_componentSelector.GetComponentsReport(
                    _currentSpecs, _currentMotor, _currentBattery, _currentController));

                var recommendations = _componentSelector.GetRecommendations(
                    _currentMotor, _currentBattery, _currentController);

                if (recommendations.Any())
                {
                    Console.WriteLine("\n⚠ РЕКОМЕНДАЦИИ:");
                    foreach (var rec in recommendations)
                    {
                        Console.WriteLine($"  • {rec}");
                    }
                }

                _currentSimulator = new BikeSimulator(
                    _currentSpecs,
                    _currentMotor,
                    _currentBattery,
                    _currentController,
                    _currentEnvironment);

                Console.WriteLine("\n✓ Симулятор готов к работе!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n❌ Ошибка при подборе компонентов: {ex.Message}");
            }
        }

        private static void ShowCurrentConfiguration()
        {
            Console.WriteLine("\n╔══════════════════════════════════════════════════════════╗");
            Console.WriteLine("║              ТЕКУЩАЯ КОНФИГУРАЦИЯ                        ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════╝");

            double componentsWeight = 0;
            if (_currentMotor != null) componentsWeight += _currentMotor.Weight;
            if (_currentBattery != null) componentsWeight += _currentBattery.Weight;
            if (_currentController != null) componentsWeight += _currentController.Weight;

            Console.WriteLine($"\n📊 ПАРАМЕТРЫ ВЕЛОСИПЕДА:");
            Console.WriteLine($"  • Вес велосипедиста: {_currentSpecs.RiderWeight:F1} кг");
            Console.WriteLine($"  • Вес велосипеда (без компонентов): {_currentSpecs.BikeWeight:F1} кг");
            Console.WriteLine($"  • Вес компонентов: {componentsWeight:F1} кг");
            Console.WriteLine($"  • ОБЩИЙ ВЕС: {_currentSpecs.TotalWeight + componentsWeight:F1} кг");
            Console.WriteLine($"  • Диаметр колес: {_currentSpecs.WheelDiameter}\"");
            Console.WriteLine($"  • Целевая скорость: {_currentSpecs.DesiredMaxSpeed} км/ч");
            Console.WriteLine($"  • Целевой пробег: {_currentSpecs.DesiredMaxRange} км");

            Console.WriteLine($"\n🌡️ ПОГОДНЫЕ УСЛОВИЯ:");
            Console.WriteLine($"  • Температура: {_currentEnvironment.Temperature}°C");
            Console.WriteLine($"  • Ветер: {_currentEnvironment.Wind.Speed} м/с, {_currentEnvironment.Wind.GetDirectionName()}");

            if (_currentMotor != null)
            {
                Console.WriteLine($"\n⚙️ КОМПЛЕКТУЮЩИЕ:");
                Console.WriteLine($"  • МОТОР: {_currentMotor.Name}");
                Console.WriteLine($"    - Мощность: {_currentMotor.Power} Вт (макс. {_currentMotor.MaxPower} Вт)");
                Console.WriteLine($"    - Напряжение: {_currentMotor.Voltage} В");
                Console.WriteLine($"    - Вес: {_currentMotor.Weight:F1} кг");

                Console.WriteLine($"  • АККУМУЛЯТОР: {_currentBattery.Name}");
                Console.WriteLine($"    - Емкость: {_currentBattery.Capacity} Ач ({_currentBattery.Capacity * _currentBattery.NominalVoltage} Вт·ч)");
                Console.WriteLine($"    - Напряжение: {_currentBattery.NominalVoltage} В");
                Console.WriteLine($"    - Макс. ток: {_currentBattery.MaxCurrent} А");
                Console.WriteLine($"    - Вес: {_currentBattery.Weight:F1} кг");

                Console.WriteLine($"  • КОНТРОЛЛЕР: {_currentController.Name}");
                Console.WriteLine($"    - Макс. ток: {_currentController.MaxCurrent} А");
                Console.WriteLine($"    - Вес: {_currentController.Weight:F1} кг");
            }
            else
            {
                Console.WriteLine("\n⚠ КОМПЛЕКТУЮЩИЕ НЕ ПОДОБРАНЫ");
                Console.WriteLine("  Выполните пункт 3 меню для подбора компонентов.");
            }
        }

        #endregion

        #region Test Methods

        private static void TestAcceleration()
        {
            if (!CheckSimulatorReady()) return;

            Console.WriteLine("\n╔══════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                     ТЕСТ РАЗГОНА                         ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════╝");

            try
            {
                _lastTestResult = _currentSimulator.TestAcceleration();

                Console.WriteLine($"\n📊 РЕЗУЛЬТАТЫ ТЕСТА РАЗГОНА:");
                Console.WriteLine($"  • Время до {_currentSpecs.DesiredMaxSpeed} км/ч: " +
                                $"{_lastTestResult.GetAccelerationTime(_currentSpecs.DesiredMaxSpeed):F1} с");
                Console.WriteLine($"  • Максимальная скорость: {_lastTestResult.MaxSpeed:F1} км/ч");
                Console.WriteLine($"  • Пиковая мощность: {_lastTestResult.GetPeakPower():F0} Вт");
                Console.WriteLine($"  • Пиковый ток: {_lastTestResult.GetPeakCurrent():F1} А");
                Console.WriteLine($"  • Влияние ветра: {_lastTestResult.AverageWindImpact:F1}%");
                Console.WriteLine($"  • Влияние температуры: {_lastTestResult.AverageTempImpact:F1}%");

                DisplaySpeedChart(_lastTestResult);
                SaveTestResult("acceleration", _lastTestResult);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n❌ Ошибка при тесте разгона: {ex.Message}");
            }
        }

        private static void TestRange()
        {
            if (!CheckSimulatorReady()) return;

            Console.WriteLine("\n╔══════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                     ТЕСТ ПРОБЕГА                         ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════╝");

            Console.Write("Введите скорость для теста (км/ч) [25]: ");
            string input = Console.ReadLine();
            double testSpeed = string.IsNullOrEmpty(input) ? 25 : double.Parse(input);

            try
            {
                _lastTestResult = _currentSimulator.TestRange(testSpeed);

                Console.WriteLine($"\n📊 РЕЗУЛЬТАТЫ ТЕСТА ПРОБЕГА:");
                Console.WriteLine($"  • Скорость: {testSpeed} км/ч");
                Console.WriteLine($"  • Пробег: {_lastTestResult.TotalDistance:F1} км");
                Console.WriteLine($"  • Время: {_lastTestResult.TotalTime / 3600:F2} ч");
                Console.WriteLine($"  • Остаток заряда: {_lastTestResult.Data.LastOrDefault()?.BatterySOC ?? 0:F1}%");
                Console.WriteLine($"  • Потреблено энергии: {_lastTestResult.GetTotalEnergyConsumed():F0} Вт·ч");
                Console.WriteLine($"  • Эффективность: {_lastTestResult.GetEnergyEfficiency():F1} км/кВт·ч");
                Console.WriteLine($"  • Влияние ветра: {_lastTestResult.AverageWindImpact:F1}%");
                Console.WriteLine($"  • Влияние температуры: {_lastTestResult.AverageTempImpact:F1}%");
                Console.WriteLine($"  • Макс. температура мотора: {_lastTestResult.GetMaxMotorTemperature():F1}°C");

                DisplayBatteryDischargeChart(_lastTestResult);
                SaveTestResult($"range_{testSpeed}kmh", _lastTestResult);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n❌ Ошибка при тесте пробега: {ex.Message}");
            }
        }

        private static bool CheckSimulatorReady()
        {
            if (_currentSimulator == null)
            {
                Console.WriteLine("\n⚠ Сначала выполните подбор комплектующих (пункт 3 в меню)!");
                return false;
            }
            return true;
        }

        #endregion

        #region Analysis Methods

        private static void AnalyzeWiring()
        {
            if (!CheckSimulatorReady()) return;

            Console.WriteLine("\n╔══════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                   АНАЛИЗ ПРОВОДКИ                        ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════╝");

            Console.Write("Длина провода от батареи к контроллеру (м) [0.5]: ");
            string btocInput = Console.ReadLine();
            double btocLength = string.IsNullOrEmpty(btocInput) ? 0.5 : double.Parse(btocInput);

            Console.Write("Длина провода от контроллера к мотору (м) [1.0]: ");
            string ctomInput = Console.ReadLine();
            double ctomLength = string.IsNullOrEmpty(ctomInput) ? 1.0 : double.Parse(ctomInput);

            try
            {
                var testResult = _currentSimulator.TestAcceleration();
                var analysis = _currentSimulator.AnalyzeWiring(testResult, btocLength, ctomLength);

                Console.WriteLine(analysis.PrintReport());

                var wireSelector = new WireSelector();
                var alternatives = wireSelector.GetAlternativeWires(
                    analysis.MaxBatteryCurrent,
                    btocLength,
                    _currentBattery.NominalVoltage);

                if (alternatives.Count > 1)
                {
                    Console.WriteLine("\n📋 АЛЬТЕРНАТИВНЫЕ ВАРИАНТЫ:");
                    foreach (var wire in alternatives.Take(3))
                    {
                        double voltageDrop = wire.CalculateVoltageDrop(analysis.MaxBatteryCurrent, btocLength);
                        Console.WriteLine($"  • {wire.CrossSectionMm2:F2} мм², " +
                                        $"падение {voltageDrop:F3}В, макс. {wire.MaxCurrentAmp}А");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n❌ Ошибка при анализе проводки: {ex.Message}");
            }
        }

        private static void AnalyzeTemperatureImpact()
        {
            if (!CheckSimulatorReady()) return;

            Console.WriteLine("\n╔══════════════════════════════════════════════════════════╗");
            Console.WriteLine("║           АНАЛИЗ ВЛИЯНИЯ ТЕМПЕРАТУРЫ                     ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════╝");

            Console.Write("Введите скорость для анализа (км/ч) [25]: ");
            string input = Console.ReadLine();
            double testSpeed = string.IsNullOrEmpty(input) ? 25 : double.Parse(input);

            try
            {
                var impact = _currentSimulator.TestTemperatureImpact(testSpeed);

                Console.WriteLine(impact.PrintReport());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n❌ Ошибка при анализе температуры: {ex.Message}");
            }
        }

        private static void AnalyzeWindImpact()
        {
            if (!CheckSimulatorReady()) return;

            Console.WriteLine("\n╔══════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                АНАЛИЗ ВЛИЯНИЯ ВЕТРА                       ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════╝");

            Console.Write("Введите скорость для анализа (км/ч) [25]: ");
            string input = Console.ReadLine();
            double testSpeed = string.IsNullOrEmpty(input) ? 25 : double.Parse(input);

            try
            {
                var test = _currentSimulator.TestWindImpact(testSpeed);

                Console.WriteLine(test.PrintReport());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n❌ Ошибка при анализе ветра: {ex.Message}");
            }
        }

        #endregion

        #region Chart Display Methods

        private static void DisplaySpeedChart(SimulationResult result)
        {
            if (result.Data.Count == 0) return;

            Console.WriteLine("\n📈 ГРАФИК СКОРОСТИ:");
            var chartData = result.GetSpeedChartData(20);

            double maxSpeed = chartData.Max(d => d.speed);
            const int chartWidth = 40;

            foreach (var point in chartData)
            {
                int bars = (int)((point.speed / maxSpeed) * chartWidth);
                Console.WriteLine($"  t={point.time,5:F1}с: {point.speed,5:F1} км/ч " +
                                $"[{new string('█', bars)}{new string('░', chartWidth - bars)}]");
            }
        }

        private static void DisplayBatteryDischargeChart(SimulationResult result)
        {
            if (result.Data.Count == 0) return;

            Console.WriteLine("\n📉 ГРАФИК РАСХОДА ЗАРЯДА:");
            var chartData = result.GetBatteryDischargeChartData(15);

            if (chartData.Count == 0)
            {
                Console.WriteLine("  Недостаточно данных для построения графика");
                return;
            }

            const int chartWidth = 40;

            foreach (var point in chartData)
            {
                int bars = (int)((point.soc / 100) * chartWidth);
                bars = Math.Max(0, Math.Min(chartWidth, bars));
                string bar = new string('█', bars);
                string empty = new string('░', chartWidth - bars);
                Console.WriteLine($"  d={point.distance,5:F1}км: {point.soc,5:F1}% [{bar}{empty}]");
            }
        }

        #endregion

        #region Export Methods

        private static void ExportData()
        {
            Console.WriteLine("\n╔══════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                   ЭКСПОРТ ДАННЫХ                         ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════╝");

            Console.WriteLine("  1. Экспорт текущей конфигурации");
            Console.WriteLine("  2. Экспорт результатов последнего теста");
            Console.WriteLine("  3. Экспорт всех данных");
            Console.Write("\n▶ Выберите вариант: ");

            string choice = Console.ReadLine();
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");

            try
            {
                switch (choice)
                {
                    case "1":
                        ExportConfiguration(timestamp);
                        break;
                    case "2":
                        ExportTestResults(timestamp);
                        break;
                    case "3":
                        ExportConfiguration(timestamp);
                        ExportTestResults(timestamp);
                        break;
                    default:
                        Console.WriteLine("❌ Неверный выбор.");
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n❌ Ошибка при экспорте: {ex.Message}");
            }
        }

        private static void ExportConfiguration(string timestamp)
        {
            string filename = $"config_{timestamp}.txt";

            using (var writer = new StreamWriter(filename))
            {
                writer.WriteLine("==========================================");
                writer.WriteLine("КОНФИГУРАЦИЯ СИМУЛЯТОРА ЭЛЕКТРОВЕЛОСИПЕДА");
                writer.WriteLine($"Дата экспорта: {DateTime.Now}");
                writer.WriteLine("==========================================");
                writer.WriteLine();

                double componentsWeight = 0;
                if (_currentMotor != null) componentsWeight += _currentMotor.Weight;
                if (_currentBattery != null) componentsWeight += _currentBattery.Weight;
                if (_currentController != null) componentsWeight += _currentController.Weight;

                writer.WriteLine("ПАРАМЕТРЫ ВЕЛОСИПЕДА:");
                writer.WriteLine($"  Вес велосипедиста: {_currentSpecs.RiderWeight} кг");
                writer.WriteLine($"  Вес велосипеда: {_currentSpecs.BikeWeight} кг");
                writer.WriteLine($"  Вес компонентов: {componentsWeight:F1} кг");
                writer.WriteLine($"  ОБЩИЙ ВЕС: {_currentSpecs.TotalWeight + componentsWeight:F1} кг");
                writer.WriteLine($"  Диаметр колес: {_currentSpecs.WheelDiameter}\"");
                writer.WriteLine($"  Желаемая скорость: {_currentSpecs.DesiredMaxSpeed} км/ч");
                writer.WriteLine($"  Желаемый пробег: {_currentSpecs.DesiredMaxRange} км");
                writer.WriteLine();

                writer.WriteLine("ПОГОДНЫЕ УСЛОВИЯ:");
                writer.WriteLine($"  Температура: {_currentEnvironment.Temperature}°C");
                writer.WriteLine($"  Ветер: {_currentEnvironment.Wind.Speed} м/с ({_currentEnvironment.Wind.GetDirectionName()})");
                writer.WriteLine();

                if (_currentMotor != null)
                {
                    writer.WriteLine("КОМПЛЕКТУЮЩИЕ:");
                    writer.WriteLine($"  МОТОР: {_currentMotor.Name}");
                    writer.WriteLine($"    Мощность: {_currentMotor.Power} Вт / {_currentMotor.MaxPower} Вт");
                    writer.WriteLine($"    Напряжение: {_currentMotor.Voltage} В");
                    writer.WriteLine($"    КПД: {_currentMotor.Efficiency:P0}");
                    writer.WriteLine($"    Вес: {_currentMotor.Weight:F1} кг");
                    writer.WriteLine();

                    writer.WriteLine($"  АККУМУЛЯТОР: {_currentBattery.Name}");
                    writer.WriteLine($"    Емкость: {_currentBattery.Capacity} Ач ({_currentBattery.Capacity * _currentBattery.NominalVoltage} Вт·ч)");
                    writer.WriteLine($"    Напряжение: {_currentBattery.NominalVoltage} В");
                    writer.WriteLine($"    Макс. ток: {_currentBattery.MaxCurrent} А");
                    writer.WriteLine($"    Вес: {_currentBattery.Weight:F1} кг");
                    writer.WriteLine();

                    writer.WriteLine($"  КОНТРОЛЛЕР: {_currentController.Name}");
                    writer.WriteLine($"    Макс. ток: {_currentController.MaxCurrent} А");
                    writer.WriteLine($"    Вес: {_currentController.Weight:F1} кг");
                }
            }

            Console.WriteLine($"\n✓ Конфигурация экспортирована в файл: {filename}");
        }

        private static void ExportTestResults(string timestamp)
        {
            if (_lastTestResult == null)
            {
                Console.WriteLine("\n⚠ Нет результатов теста для экспорта!");
                return;
            }

            string filename = $"test_result_{timestamp}.csv";
            File.WriteAllText(filename, _lastTestResult.ToCsv());
            Console.WriteLine($"\n✓ Результаты теста экспортированы в файл: {filename}");

            string reportFilename = $"test_report_{timestamp}.txt";
            var summary = _lastTestResult.GetSummary();
            File.WriteAllText(reportFilename, summary.GetReport());
            Console.WriteLine($"✓ Отчет экспортирован в файл: {reportFilename}");
        }

        private static void SaveTestResult(string testName, SimulationResult result)
        {
            string filename = $"{testName}_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
            File.WriteAllText(filename, result.ToCsv());
            Console.WriteLine($"\n✓ Результаты сохранены в файл: {filename}");
        }

        #endregion
    }
}
