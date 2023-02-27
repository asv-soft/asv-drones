namespace Asv.Drones.Gui.Core
{

    public interface IAppInfo
    {
        string Name { get; }
        string Version { get; }
        string Author { get; }
        string AppUrl { get; }
        string AppLicense { get; }
        string CurrentAvaloniaVersion { get; }
    }

    public interface IAppPathInfo
    {
        string ApplicationDataFolder { get; }
        string ApplicationConfigFilePath { get; }
        string MapCacheFolder { get; }
    }


    public interface IAppService
    {
        IAppInfo Info { get; }
        IAppPathInfo Paths { get; }
    }

    
}