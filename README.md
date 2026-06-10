# Power Monitor App

A modern Windows desktop application that polls hardware sensors and accurately displays real-time system power usage (CPU, GPU, etc.) utilizing WinUI 3 and LibreHardwareMonitorLib.

## Features
- **Real-Time Tracking**: Provides live load and power metrics for major hardware components.
- **Total System Power**: Aggregates various power limits across devices to output a comprehensive total wattage number.
- **Auto-Updates**: Integrated with Velopack for automatic and seamless updates sourced directly from GitHub Releases.
- **Modern UI**: Dark/Light mode integration using the Windows 11 Fluent design principles and Mica backdrops.

## Installation
1. Go to the [Releases](https://github.com/AgastyaShenoy/power-monitor-app/releases) page.
2. Download the latest `Setup.exe`.
3. Run the installer. 
*Note: You may be prompted by User Account Control (UAC) during execution, as Administrator privileges are strictly required to read kernel-level hardware power sensors.*

## Building Locally
1. Ensure you have the .NET 8 SDK and WinUI 3 workloads installed.
2. Clone the repository.
3. Open a terminal and run:
```bash
dotnet build
dotnet run
```
To build the setup installer, install the Velopack CLI (`dotnet tool install -g vpk`), publish the project, and run `vpk pack`.

## License
This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
