using System.Composition.Hosting;
using Asv.Cfg;
using Asv.Common;
using Avalonia.Controls.Templates;

namespace Asv.Drones.Gui.Api;

/// <summary>
/// Application host
/// </summary>
public interface IApplicationHost
{
    /// <summary>
    /// Application args
    /// </summary>
    IAppArgs Args { get; }
    /// <summary>
    /// Base information about current application
    /// </summary>
    IAppInfo Info { get; }

    /// <summary>
    /// Path helper
    /// </summary>
    IAppPathInfo Paths { get; }

    /// <summary>
    /// Host for add data templates 
    /// </summary>
    IDataTemplateHost DataTemplateHost { get; }

    /// <summary>
    /// Configuration of the application
    /// </summary>
    IConfiguration Configuration { get; }

    ILocalizationService Localization { get; }
    ILogService Logs { get; }
    IPluginManager PluginManager { get; }

    /// <summary>
    /// IoC container
    /// </summary>
    CompositionHost Container { get; }

    /// <summary>
    /// Gets an enumerable collection of theme items.
    /// </summary>
    /// <value>
    /// An enumerable collection of theme items.
    /// </value>
    IEnumerable<IThemeInfo> Themes { get; }

    /// <summary>
    /// Gets the current theme of the application.
    /// </summary>
    /// <remarks>
    /// This property returns an instance of <see cref="IRxEditableValue{TValue}"/> which represents the current theme.
    /// </remarks>
    /// <value>
    /// An <see cref="IRxEditableValue{T}"/> instance representing the current theme of the application.
    /// </value>
    IRxEditableValue<IThemeInfo> CurrentTheme { get; }
    /// <summary>
    /// Main application view. Can be NULL! before main activity is loading
    /// </summary>
    IShell? Shell { get; }
    /// <summary>
    /// Base class for all user dialogs
    /// Can be NULL! before main activity is loading
    /// </summary>
    IDialogService? Dialogs { get; }
    
    /// <summary>
    /// Try to restart application
    /// </summary>
    void RestartApplication();
}