namespace Asv.Drones.Gui.Core
{
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
}