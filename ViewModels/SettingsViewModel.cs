using CommunityToolkit.Mvvm.ComponentModel;
using PowerMonitorApp.Services;

namespace PowerMonitorApp.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    [ObservableProperty]
    private string theme = SettingsService.Theme;

    [ObservableProperty]
    private bool autoUpdate = SettingsService.AutoUpdate;

    partial void OnThemeChanged(string value)
    {
        SettingsService.Theme = value;
    }

    partial void OnAutoUpdateChanged(bool value)
    {
        SettingsService.AutoUpdate = value;
    }
}
