namespace Asv.Drones.Gui.Core;

public interface IAppInfo
{
    string Name { get; }
    string Version { get; }
    string Author { get; }
    string AppUrl { get; }
    string AppLicense { get; }
    string CurrentAvaloniaVersion { get; }
}

public class AppInfo : IAppInfo
{
    public AppInfo(string name, string version, string author, string appUrl, string appLicense, string avaloniaVersion)
    {
        Name = name;
        Version = version;
        Author = author;
        AppUrl = appUrl;
        AppLicense = appLicense;
        CurrentAvaloniaVersion = avaloniaVersion;
    }

    public string Name { get; }
    public string Version { get; }
    public string Author { get; }
    public string AppUrl { get; }
    public string AppLicense { get; }
    public string CurrentAvaloniaVersion { get; }
       
}