using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Dispatching;
using PowerMonitorApp.Models;
using PowerMonitorApp.Services;

namespace PowerMonitorApp.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly HardwareService _hardwareService;

    public ObservableCollection<HardwareItem> HardwareComponents { get; } = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SystemPowerFormatted))]
    private float totalSystemPower;

    public string SystemPowerFormatted => $"{TotalSystemPower:F1} W";

    public MainViewModel()
    {
        _hardwareService = new HardwareService(DispatcherQueue.GetForCurrentThread());
        _hardwareService.OnHardwareUpdated = UpdateHardware;
        _hardwareService.StartPolling(1000);
    }

    private void UpdateHardware(System.Collections.Generic.List<HardwareItem> updatedHardware)
    {
        foreach (var updatedHw in updatedHardware)
        {
            var existingHw = HardwareComponents.FirstOrDefault(h => h.Name == updatedHw.Name);
            if (existingHw == null)
            {
                HardwareComponents.Add(updatedHw);
            }
            else
            {
                existingHw.TotalLoad = updatedHw.TotalLoad;
                existingHw.TotalPower = updatedHw.TotalPower;

                foreach (var updatedSensor in updatedHw.Sensors)
                {
                    var existingSensor = existingHw.Sensors.FirstOrDefault(s => s.Name == updatedSensor.Name && s.SensorType == updatedSensor.SensorType);
                    if (existingSensor == null)
                    {
                        existingHw.Sensors.Add(updatedSensor);
                    }
                    else
                    {
                        existingSensor.Value = updatedSensor.Value;
                        existingSensor.HasProgressBar = updatedSensor.HasProgressBar;
                    }
                }
            }
        }
        TotalSystemPower = HardwareComponents.Sum(h => h.TotalPower);
    }
}
