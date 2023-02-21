namespace Asv.Drones.Gui.Core
{
    public class AppPathInfo : IAppPathInfo
    {
        public AppPathInfo(string applicationDataFolder, string applicationConfigFilePath, string mapCacheFolder)
        {
            ApplicationDataFolder = applicationDataFolder;
            ApplicationConfigFilePath = applicationConfigFilePath;
            MapCacheFolder = mapCacheFolder;
        }

        public string ApplicationDataFolder { get; }
        public string ApplicationConfigFilePath { get; }
        public string MapCacheFolder { get; }
    }
}