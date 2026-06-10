using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using LibreHardwareMonitor.Hardware;
using Microsoft.UI.Dispatching;
using PowerMonitorApp.Models;

namespace PowerMonitorApp.Services;

public class HardwareService : IDisposable
{
    private readonly Computer _computer;
    private readonly DispatcherQueue _dispatcherQueue;
    private Timer? _timer;

    public Action<List<HardwareItem>>? OnHardwareUpdated { get; set; }

    public HardwareService(DispatcherQueue dispatcherQueue)
    {
        _dispatcherQueue = dispatcherQueue;
        _computer = new Computer
        {
            IsCpuEnabled = true,
            IsGpuEnabled = true,
            IsMemoryEnabled = true,
            IsMotherboardEnabled = true,
            IsNetworkEnabled = false,
            IsControllerEnabled = false,
            IsStorageEnabled = true
        };
        _computer.Open();
    }

    public void StartPolling(int intervalMs = 1000)
    {
        _timer = new Timer(PollHardware, null, 0, intervalMs);
    }

    private class UpdateVisitor : IVisitor
    {
        public void VisitComputer(IComputer computer)
        {
            computer.Traverse(this);
        }

        public void VisitHardware(IHardware hardware)
        {
            hardware.Update();
            foreach (IHardware subHardware in hardware.SubHardware) subHardware.Accept(this);
        }

        public void VisitSensor(ISensor sensor) { }
        public void VisitParameter(IParameter parameter) { }
    }

    private void PollHardware(object? state)
    {
        _computer.Accept(new UpdateVisitor());

        var hardwareList = new List<HardwareItem>();

        foreach (var hw in _computer.Hardware)
        {
            var hwItem = new HardwareItem();
            string hwTypeStr = hw.HardwareType.ToString();
            string displayName = hw.Name;
            int priority = 99;

            if (hwTypeStr.Contains("Cpu")) {
                hwItem.HardwareType = "CPU";
                displayName = "CPU";
                priority = 1;
            }
            else if (hwTypeStr.Contains("Gpu")) {
                hwItem.HardwareType = "GPU";
                displayName = "GPU";
                priority = 2;
            }
            else if (hwTypeStr.Contains("Memory")) {
                hwItem.HardwareType = "Memory";
                displayName = "Memory";
                priority = 3;
            }
            else if (hwTypeStr.Contains("Storage")) {
                hwItem.HardwareType = "Storage";
                priority = 4;
            }
            else if (hwTypeStr.Contains("Motherboard")) {
                hwItem.HardwareType = "Motherboard";
                priority = 6;
            }
            else {
                hwItem.HardwareType = "Other";
            }
            
            hwItem.Name = displayName;

            var parkedCores = new System.Collections.Generic.HashSet<string>();

            // First pass for CPU parked cores
            if (hwItem.HardwareType == "CPU")
            {
                foreach (var sensor in hw.Sensors)
                {
                    if (sensor.Value.HasValue)
                    {
                        var match = System.Text.RegularExpressions.Regex.Match(sensor.Name, @"(Core\s*(?:#\s*)?\d+)");
                        if (match.Success)
                        {
                            string coreId = match.Groups[1].Value.Trim();
                            if ((sensor.SensorType == SensorType.Load && sensor.Value.Value == 0.0) ||
                                (sensor.Name.Contains("Residency") && sensor.Name.Contains("C") && sensor.Value.Value > 95.0))
                            {
                                parkedCores.Add(coreId);
                            }
                        }
                    }
                }
            }

            float hwTotalPower = 0;
            float hwTotalLoad = 0;

            foreach (var sensor in hw.Sensors)
            {
                if (sensor.Value.HasValue)
                {
                    string sName = sensor.Name;
                    
                    if (sensor.SensorType == SensorType.Power)
                    {
                        if (hwItem.HardwareType == "CPU")
                        {
                            if (sName.Contains("Package", StringComparison.OrdinalIgnoreCase))
                            {
                                hwTotalPower = (float)sensor.Value.Value;
                            }
                            else if (hwTotalPower == 0 || hwTotalPower < (float)sensor.Value.Value)
                            {
                                hwTotalPower = (float)sensor.Value.Value;
                            }
                        }
                        else
                        {
                            // For GPU and other hardware, take the maximum Power sensor as the total
                            if (hwTotalPower < (float)sensor.Value.Value) 
                                hwTotalPower = (float)sensor.Value.Value;
                        }
                    }

                    if (hwItem.HardwareType == "CPU")
                    {
                        var match = System.Text.RegularExpressions.Regex.Match(sName, @"(Core\s*(?:#\s*)?\d+)");
                        if (match.Success && parkedCores.Contains(match.Groups[1].Value.Trim()))
                        {
                            if (!sName.Contains("(Parked)"))
                                sName += " (Parked)";
                        }
                    }

                    string unit = sensor.SensorType switch
                    {
                        SensorType.Temperature => "°C",
                        SensorType.Load => "%",
                        SensorType.Power => "W",
                        SensorType.Voltage => "V",
                        SensorType.Clock => "MHz",
                        SensorType.Data => "GB",
                        SensorType.SmallData => "MB",
                        SensorType.Fan => "RPM",
                        _ => ""
                    };

                    bool hasBar = false;
                    if (sensor.SensorType == SensorType.Load)
                    {
                        if (hwItem.HardwareType == "CPU" && sName.Contains("Total", StringComparison.OrdinalIgnoreCase))
                        {
                            hwTotalLoad = (float)sensor.Value.Value;
                        }
                        else if (hwItem.HardwareType == "GPU" && sName.Equals("GPU Core", StringComparison.OrdinalIgnoreCase))
                        {
                            hwTotalLoad = (float)sensor.Value.Value;
                        }
                        else if (hwItem.HardwareType == "Memory" && sName.Equals("Memory", StringComparison.OrdinalIgnoreCase))
                        {
                            hwTotalLoad = (float)sensor.Value.Value;
                        }
                    }

                    hwItem.Sensors.Add(new SensorItem
                    {
                        Name = sName,
                        SensorType = sensor.SensorType.ToString(),
                        Value = (float)Math.Round(sensor.Value.Value, 2),
                        Unit = unit,
                        HasProgressBar = false // We removed individual progress bars
                    });
                }
            }

            hwItem.TotalLoad = hwTotalLoad;
            hwItem.TotalPower = hwTotalPower;

            if (hwItem.Sensors.Count > 0)
            {
                hardwareList.Add(hwItem);
            }
        }

        var sorted = hardwareList.OrderBy(h => 
        {
            if (h.HardwareType == "CPU") return 1;
            if (h.HardwareType == "GPU") return 2;
            if (h.HardwareType == "Memory") return 3;
            if (h.HardwareType == "Storage") return 4;
            return 99;
        }).ToList();

        _dispatcherQueue.TryEnqueue(() =>
        {
            OnHardwareUpdated?.Invoke(sorted);
        });
    }

    public void Dispose()
    {
        _timer?.Dispose();
        _computer.Close();
    }
}
