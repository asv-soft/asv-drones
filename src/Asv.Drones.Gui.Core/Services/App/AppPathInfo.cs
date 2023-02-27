namespace Asv.Drones.Gui.Core
{
    public class AppPathInfo : IAppPathInfo
    {
        public AppPathInfo(string applicationDataFolder, string applicationConfigFilePath)
        {
            ApplicationDataFolder = applicationDataFolder;
            ApplicationConfigFilePath = applicationConfigFilePath;
        }

        public string ApplicationDataFolder { get; }
        public string ApplicationConfigFilePath { get; }
        
    }
}