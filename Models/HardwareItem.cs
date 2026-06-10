using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace PowerMonitorApp.Models;

public partial class HardwareItem : ObservableObject
{
    [ObservableProperty]
    private string name = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TotalLoadVisibility))]
    private string hardwareType = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TotalLoadFormatted))]
    private float totalLoad;

    public string TotalLoadFormatted => $"{TotalLoad:F1}%";

    [ObservableProperty]
    private float totalPower;

    public Microsoft.UI.Xaml.Visibility TotalLoadVisibility => HardwareType == "CPU" || HardwareType == "GPU" || HardwareType == "Memory" ? Microsoft.UI.Xaml.Visibility.Visible : Microsoft.UI.Xaml.Visibility.Collapsed;

    public ObservableCollection<SensorItem> Sensors { get; } = new();
}
