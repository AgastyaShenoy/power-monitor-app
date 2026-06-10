using System;
using Windows.Storage;

namespace PowerMonitorApp.Services;

public class SettingsService
{
    private static readonly ApplicationDataContainer _localSettings = ApplicationData.Current.LocalSettings;

    public static string Theme
    {
        get => _localSettings.Values["Theme"] as string ?? "Default";
        set => _localSettings.Values["Theme"] = value;
    }

    public static bool AutoUpdate
    {
        get => _localSettings.Values["AutoUpdate"] as bool? ?? true;
        set => _localSettings.Values["AutoUpdate"] = value;
    }
}
