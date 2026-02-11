using EBike_Simulator.Core.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace EBike_Simulator.Core.Models.Simulation
{
    public class SimulationResult
    {
        #region props

        public List<SimulationData> Data { get; set; } = new();
        public double TotalDistance { get; set; }
        public double TotalTime { get; set; }
        public double MaxSpeed { get; set; }
        public bool BatteryEmpty { get; set; }
        public double AverageWindImpact { get; set; }
        public double AverageTempImpact { get; set; }
        public double FinalBatteryTemp { get; set; }

        #endregion

        #region methods

        public double GetAccelerationTime(double targetSpeed) =>
            Data.FirstOrDefault(p => p.Speed >= targetSpeed)?.Time ?? 0;

        public double GetAverageSpeed() => TotalTime > 0 ? (TotalDistance / (TotalTime / 3600)) : 0;
        public double GetAveragePower() => Data.Count > 0 ? Data.Average(d => d.Power) : 0;
        public double GetAverageCurrent() => Data.Count > 0 ? Data.Average(d => d.Current) : 0;

        public double GetTotalEnergyConsumed()
        {
            if (Data.Count < 2) return 0;

            double totalEnergy = 0;
            for (int i = 1; i < Data.Count; i++)
            {
                double dt = Data[i].Time - Data[i - 1].Time;
                double avgPower = (Data[i].Power + Data[i - 1].Power) / 2;
                totalEnergy += avgPower * (dt / 3600);
            }
            return totalEnergy;
        }

        public double GetEnergyEfficiency()
        {
            double energy = GetTotalEnergyConsumed();
            if (energy <= 0 || TotalDistance <= 0) return 0;
            return TotalDistance / (energy / 1000);
        }

        public double GetMaxMotorTemperature() => Data.Count > 0 ? Data.Max(d => d.MotorTemp) : 0;
        public double GetMaxControllerTemperature() => Data.Count > 0 ? Data.Max(d => d.ControllerTemp) : 0;
        public double GetPeakPower() => Data.Count > 0 ? Data.Max(d => d.Power) : 0;
        public double GetPeakCurrent() => Data.Count > 0 ? Data.Max(d => d.Current) : 0;

        public (double normalTime, double warningTime, double criticalTime) GetThermalOperatingTimes()
        {
            if (Data.Count < 2) return (0, 0, 0);

            double normal = 0, warning = 0, critical = 0;
            for (int i = 1; i < Data.Count; i++)
            {
                double dt = Data[i].Time - Data[i - 1].Time;
                switch (Data[i].GetThermalStatus())
                {
                    case ThermalStatus.Normal: normal += dt; break;
                    case ThermalStatus.Warning: warning += dt; break;
                    case ThermalStatus.Critical: critical += dt; break;
                }
            }
            return (normal, warning, critical);
        }

        public List<(double time, double speed)> GetSpeedChartData(int maxPoints = 50)
        {
            var result = new List<(double, double)>();
            if (Data.Count == 0) return result;

            int step = Math.Max(1, Data.Count / Math.Min(maxPoints, Data.Count));
            for (int i = 0; i < Data.Count; i += step)
                result.Add((Data[i].Time, Data[i].Speed));

            if (Data.Count > 0 && !result.Any(p => Math.Abs(p.Item1 - Data.Last().Time) < 0.001))
                result.Add((Data.Last().Time, Data.Last().Speed));

            return result;
        }

        public SimulationData FindPointAtSOC(double targetSOC) =>
            Data.FirstOrDefault(d => d.BatterySOC <= targetSOC);

        public SimulationData FindPointAtTemperature(double targetTemp, ComponentType component)
        {
            return component switch
            {
                ComponentType.Motor => Data.FirstOrDefault(d => d.MotorTemp >= targetTemp),
                ComponentType.Controller => Data.FirstOrDefault(d => d.ControllerTemp >= targetTemp),
                ComponentType.Battery => Data.FirstOrDefault(d => d.BatteryTemp >= targetTemp),
                _ => null
            };
        }

        public bool HasReachedSpeed(double targetSpeed) =>
            Data.Any(d => d.Speed >= targetSpeed * 0.95);

        public SimulationSummary GetSummary()
        {
            var summary = new SimulationSummary();
            if (Data.Count == 0) return summary;

            var thermalTimes = GetThermalOperatingTimes();

            summary.TotalDistance = TotalDistance;
            summary.TotalTime = TotalTime;
            summary.MaxSpeed = MaxSpeed;
            summary.AverageSpeed = GetAverageSpeed();
            summary.AveragePower = GetAveragePower();
            summary.PeakPower = GetPeakPower();
            summary.EnergyConsumed = GetTotalEnergyConsumed();
            summary.EnergyEfficiency = GetEnergyEfficiency();
            summary.MaxMotorTemp = GetMaxMotorTemperature();
            summary.MaxControllerTemp = GetMaxControllerTemperature();
            summary.FinalBatteryTemp = FinalBatteryTemp;
            summary.BatteryDepleted = BatteryEmpty;
            summary.WindImpact = AverageWindImpact;
            summary.TempImpact = AverageTempImpact;
            summary.NormalOperatingTime = thermalTimes.normalTime;
            summary.WarningOperatingTime = thermalTimes.warningTime;
            summary.CriticalOperatingTime = thermalTimes.criticalTime;

            return summary;
        }

        public string ToCsv()
        {
            if (Data.Count == 0) return "";

            var lines = new List<string>
            {
                "Time(s),Speed(km/h),Distance(km),MotorTemp(C),ControllerTemp(C),BatteryTemp(C),BatterySOC(%),Current(A),Power(W),WindEffect(%),TempEffect(%)"
            };

            foreach (var point in Data)
            {
                lines.Add($"{point.Time:F2},{point.Speed:F1},{point.Distance:F3}," +
                         $"{point.MotorTemp:F1},{point.ControllerTemp:F1},{point.BatteryTemp:F1}," +
                         $"{point.BatterySOC:F1},{point.Current:F1},{point.Power:F1}," +
                         $"{point.WindEffect:F1},{point.TempEffect:F1}");
            }

            return string.Join(System.Environment.NewLine, lines);
        }

        public static SimulationResult FromCsv(string csvData)
        {
            var result = new SimulationResult();
            if (string.IsNullOrEmpty(csvData)) return result;

            var lines = csvData.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 1; i < lines.Length; i++)
            {
                var parts = lines[i].Split(',');
                if (parts.Length < 10) continue;

                result.Data.Add(new SimulationData
                {
                    Time = double.Parse(parts[0]),
                    Speed = double.Parse(parts[1]),
                    Distance = double.Parse(parts[2]),
                    MotorTemp = double.Parse(parts[3]),
                    ControllerTemp = double.Parse(parts[4]),
                    BatteryTemp = double.Parse(parts[5]),
                    BatterySOC = double.Parse(parts[6]),
                    Current = double.Parse(parts[7]),
                    Power = double.Parse(parts[8]),
                    WindEffect = parts.Length > 9 ? double.Parse(parts[9]) : 0,
                    TempEffect = parts.Length > 10 ? double.Parse(parts[10]) : 0
                });
            }

            if (result.Data.Count > 0)
            {
                result.TotalTime = result.Data.Max(d => d.Time);
                result.TotalDistance = result.Data.Max(d => d.Distance);
                result.MaxSpeed = result.Data.Max(d => d.Speed);
                result.BatteryEmpty = result.Data.Last().BatterySOC <= 1;
                result.AverageWindImpact = result.Data.Average(d => d.WindEffect);
                result.AverageTempImpact = result.Data.Average(d => d.TempEffect);
                result.FinalBatteryTemp = result.Data.Last().BatteryTemp;
            }

            return result;
        }

        #endregion
    }
}
