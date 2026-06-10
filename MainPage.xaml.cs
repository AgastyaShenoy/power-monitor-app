using Microsoft.UI.Xaml.Controls;
using PowerMonitorApp.ViewModels;

namespace PowerMonitorApp;

public sealed partial class MainPage : Page
{
    public MainViewModel ViewModel { get; }

    public MainPage()
    {
        ViewModel = new MainViewModel();
        this.InitializeComponent();
    }
}
