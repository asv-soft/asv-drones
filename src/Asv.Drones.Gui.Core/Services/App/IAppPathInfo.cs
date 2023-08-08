namespace Asv.Drones.Gui.Core;

public interface IAppPathInfo
{
    string DataFolder { get; }
    string ConfigFilePath { get; }
    string StoreFilePath { get; }
}

public class AppPathInfo : IAppPathInfo
{
    public AppPathInfo(string dataFolder, string configFilePath, string storeFilePath)
    {
        DataFolder = dataFolder;
        ConfigFilePath = configFilePath;
        StoreFilePath = storeFilePath;
    }

    public string DataFolder { get; }
    public string ConfigFilePath { get; }
    public string StoreFilePath { get; }
}
