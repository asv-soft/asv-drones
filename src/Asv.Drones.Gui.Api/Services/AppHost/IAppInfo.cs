namespace Asv.Drones.Gui.Api;

public interface IAppInfo
{
    /// <summary>
    /// Application title
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Application version
    /// </summary>
    string Version { get; }

    /// <summary>
    /// Authors
    /// </summary>
    string Author { get; }

    /// <summary>
    /// Application home page URL
    /// </summary>
    string AppUrl { get; }

    /// <summary>
    /// Licence name
    /// </summary>
    string AppLicense { get; }

    /// <summary>
    /// Avalonia UI package version
    /// </summary>
    string AvaloniaVersion { get; }
}