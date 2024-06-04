namespace Asv.Drones.Gui.Api;

/// <summary>
///  Information about the application's path
/// </summary>
public interface IAppPathInfo
{
    /// <summary>
    ///  The folder where the application stores its data.
    ///  This is the folder where the application stores its data, such as configuration files, logs, and plugins.
    ///  Folder is created by the application if it does not exist.
    ///  The folder is created in the user's home directory and will not be deleted when the application is uninstalled or updated.
    /// </summary>
    string AppDataFolder { get; }
}