using CommunityToolkit.Mvvm.ComponentModel;

namespace PowerMonitorApp.Models;

public partial class SensorItem : ObservableObject
{
    [ObservableProperty]
    private string name = string.Empty;

    [ObservableProperty]
    private string sensorType = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ProgressValue))]
    private float value;

    [ObservableProperty]
    private string unit = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ProgressBarVisibility))]
    private bool hasProgressBar;

    public double ProgressValue => Value;

    public Microsoft.UI.Xaml.Visibility ProgressBarVisibility => HasProgressBar ? Microsoft.UI.Xaml.Visibility.Visible : Microsoft.UI.Xaml.Visibility.Collapsed;
}
