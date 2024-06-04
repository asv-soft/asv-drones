using Avalonia.Controls;

namespace Asv.Drones.Gui.Api;

/// <summary>
/// IMapAction provides a contract for a Map Action.
/// It's part of the Asv.Drones.Gui.Core project which uses AvaloniaUI for the User Interface.
/// </summary>
public interface IMapAction : IViewModel
{
    /// <summary>
    /// Position on the control where an element/border should be docked.
    /// </summary>
    Dock Dock { get; }

    /// <summary>
    /// Represents the order or sequence of the map actions.
    /// </summary>
    int Order { get; }

    /// <summary>
    /// Initializes a map action using the provided context.
    /// </summary>
    /// <param name="context">An instance of IMap to be used for initiation.</param>
    /// <returns>An instance of IMapAction.</returns>
    IMapAction Init(IMap context);
}