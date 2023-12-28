namespace Asv.Drones.Gui.Core;

/// <summary>
/// Represents an interface for retrieving information about an application.
/// </summary>
public interface IAppInfo
{
    /// <summary>
    /// Gets the name of the property.
    /// </summary>
    /// <returns>
    /// The name.
    /// </returns>
    string Name { get; }

    /// <summary>
    /// Gets the version of the property.
    /// </summary>
    /// <value>
    /// A string representing the version of the property.
    /// </value>
    string Version { get; }

    /// <summary>
    /// Gets the name of the author of the code.
    /// </summary>
    /// <returns>A string representing the name of the author.</returns>
    string Author { get; }

    /// <summary>
    /// Gets the URL of the application.
    /// </summary>
    /// <remarks>
    /// This property is used to retrieve the URL of the application.
    /// </remarks>
    /// <value>
    /// The URL of the application.
    /// </value>
    string AppUrl { get; }

    /// <summary>
    /// Gets the license information of the application.
    /// </summary>
    /// <returns>
    /// The license string of the application.
    /// </returns>
    string AppLicense { get; }

    /// <summary>
    /// Gets the current version of Avalonia.
    /// </summary>
    /// <returns>The current version of Avalonia.</returns>
    string CurrentAvaloniaVersion { get; }
}

/// <summary>
/// Represents information about an application.
/// </summary>
public class AppInfo : IAppInfo
{
    /// <summary>
    /// Represents information about an application.
    /// </summary>
    public AppInfo(string name, string version, string author, string appUrl, string appLicense, string avaloniaVersion)
    {
        Name = name;
        Version = version;
        Author = author;
        AppUrl = appUrl;
        AppLicense = appLicense;
        CurrentAvaloniaVersion = avaloniaVersion;
    }

    /// <summary>
    /// Gets the name of an object.
    /// </summary>
    /// <value>
    /// The name.
    /// </value>
    public string Name { get; }

    /// <summary>
    /// Gets the version of the property.
    /// </summary>
    /// <returns>The version of the property as a string.</returns>
    public string Version { get; }

    /// <summary>
    /// Gets the author of the property.
    /// </summary>
    /// <value>
    /// The author of the property.
    /// </value>
    public string Author { get; }

    /// <summary>
    /// Gets the URL of the property app.
    /// </summary>
    /// <returns>The URL of the property app.</returns>
    public string AppUrl { get; }

    /// <summary>
    /// Gets the license information of the application.
    /// </summary>
    /// <value>
    /// A string representing the license of the application.
    /// </value>
    public string AppLicense { get; }

    /// <summary>
    /// Gets the current version of Avalonia.
    /// </summary>
    /// <value>
    /// A string representing the current version of Avalonia.
    /// </value>
    public string CurrentAvaloniaVersion { get; }
       
}