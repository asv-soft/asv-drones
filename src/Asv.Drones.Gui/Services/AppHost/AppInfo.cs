using Asv.Drones.Gui.Api;

namespace Asv.Drones.Gui;

public class AppInfo : IAppInfo
{
    private static AppInfo? _instance;

    public static AppInfo DesignTimeInstance => _instance ??= new AppInfo
    {
        Version = "0.0.0",
        Name = "Asv.Drones",
        Author = "Asv Soft",
        AppLicense = "MIT License",
        AppUrl = "https://github.com/asvol/asv-drones",
        AvaloniaVersion = "0.0.0",
    };

    public required string Name { get; set; }
    public required string Version { get; set; }
    public required string Author { get; set; }
    public required string AppUrl { get; set; }
    public required string AppLicense { get; set; }
    public required string AvaloniaVersion { get; set; }
}