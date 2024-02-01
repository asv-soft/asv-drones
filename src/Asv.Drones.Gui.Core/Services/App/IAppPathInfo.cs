namespace Asv.Drones.Gui.Core;

/// <summary>
/// Represents an interface for retrieving application path information.
/// </summary>
public interface IAppPathInfo
{
    /// <summary>
    /// Gets the path of the data folder.
    /// </summary>
    /// <value>
    /// The path of the data folder.
    /// </value>
    string DataFolder { get; }

    /// <summary>
    /// Gets the file path to the configuration file.
    /// </summary>
    /// <returns>A string representing the file path to the configuration file.</returns>
    string ConfigFilePath { get; }

    /// <summary>
    /// Gets the file path of the store.
    /// </summary>
    /// <value>
    /// The file path of the store.
    /// </value>
    string StoreFilePath { get; }
}

/// <summary>
/// Represents the information related to the application paths.
/// </summary>
public class AppPathInfo : IAppPathInfo
{
    /// <summary>
    /// Represents the information about the application paths.
    /// </summary>
    public AppPathInfo(string dataFolder, string configFilePath, string storeFilePath)
    {
        DataFolder = dataFolder;
        ConfigFilePath = configFilePath;
        StoreFilePath = storeFilePath;
    }

    /// <summary>
    /// Gets the path of the data folder.
    /// </summary>
    /// <value>
    /// A string representing the path of the data folder.
    /// </value>
    public string DataFolder { get; }

    /// <summary>
    /// Gets the path to the configuration file.
    /// </summary>
    /// <value>
    /// The path to the configuration file.
    /// </value>
    public string ConfigFilePath { get; }

    /// <summary>
    /// Gets the file path of the store.
    /// </summary>
    /// <remarks>
    /// The store file path represents the location of the store file.
    /// </remarks>
    /// <value>
    /// The file path of the store.
    /// </value>
    public string StoreFilePath { get; }
}
