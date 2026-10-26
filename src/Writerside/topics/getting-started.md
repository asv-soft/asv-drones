# Getting Started

Asv.Drones is a modular application built with .NET and Avalonia. The desktop
project is the supported entry point for local development.

## Requirements

- .NET 10 SDK.
- Git.
- Windows, macOS, or Linux for desktop development.
- A C# editor or IDE, such as JetBrains Rider or Visual Studio.

The solution also contains Android and iOS projects. Building the entire solution
requires the corresponding .NET workloads and platform tools. Use the desktop
project when those workloads are not installed.

## Clone and Build

Run these commands from the directory where you keep your source repositories:

```bash
git clone https://github.com/asv-soft/asv-drones.git
cd asv-drones
dotnet restore src/Asv.Drones.Desktop/Asv.Drones.Desktop.csproj
dotnet build src/Asv.Drones.Desktop/Asv.Drones.Desktop.csproj --no-restore
```

## Run the Desktop Application

From the repository root:

```bash
dotnet run --project src/Asv.Drones.Desktop/Asv.Drones.Desktop.csproj
```

The desktop project references the application and API projects. It uses the
versions declared in `src/Directory.Build.props`; changing the documentation does
not change the installed application or package versions.

## Build the Launcher

The launcher is a separate project:

```bash
dotnet restore src/Asv.Drones.Launcher/Asv.Drones.Launcher.csproj
dotnet build src/Asv.Drones.Launcher/Asv.Drones.Launcher.csproj --no-restore
```

For project responsibilities and contribution checks, see [Development](development.md).
