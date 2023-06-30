namespace Asv.Drones.Gui.Core;

public interface IAppPathInfo
{
    string ApplicationDataFolder { get; }
    string ApplicationConfigFilePath { get; }
    string AppStoreFolder { get; }
}

public class AppPathInfo : IAppPathInfo
{
    public AppPathInfo(string applicationDataFolder, string applicationConfigFilePath, string appStoreFolder)
    {
        ApplicationDataFolder = applicationDataFolder;
        ApplicationConfigFilePath = applicationConfigFilePath;
        AppStoreFolder = appStoreFolder;
    }

    public string ApplicationDataFolder { get; }
    public string ApplicationConfigFilePath { get; }
    public string AppStoreFolder { get; }
}
