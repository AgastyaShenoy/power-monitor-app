using Microsoft.UI.Xaml.Controls;
using PowerMonitorApp.ViewModels;
using System;

namespace PowerMonitorApp;

public sealed partial class SettingsPage : Page
{
    public SettingsViewModel ViewModel { get; } = new SettingsViewModel();

    public SettingsPage()
    {
        this.InitializeComponent();
    }

    private async void CheckUpdates_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        try
        {
            var mgr = new Velopack.UpdateManager("https://github.com/AgastyaShenoy/power-monitor-app");
            var updateInfo = await mgr.CheckForUpdatesAsync();
            if (updateInfo != null)
            {
                await mgr.DownloadUpdatesAsync(updateInfo);
                mgr.ApplyUpdatesAndRestart(updateInfo);
            }
            else
            {
                var dialog = new ContentDialog
                {
                    Title = "Up to Date",
                    Content = "You are running the latest version.",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };
                await dialog.ShowAsync();
            }
        }
        catch (Exception ex)
        {
            var dialog = new ContentDialog
            {
                Title = "Error",
                Content = $"Could not check for updates: {ex.Message}",
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot
            };
            await dialog.ShowAsync();
        }
    }
}
