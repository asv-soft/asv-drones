using Material.Icons;

namespace Asv.Drones.Gui.Api;

/// <summary>
/// Enumeration to specify the location of a widget on the map.
/// </summary>
public enum WidgetLocation
{
    Left,
    Right,
    Bottom
}

/// <summary>
/// An interface that describes the structure and behavior of a Map Widget.
/// Map Widget represents an interface element on the map.
/// </summary>
public interface IMapWidget : IViewModel
{
    /// <summary>
    /// Gets the location of the widget on the map.
    /// </summary>
    WidgetLocation Location { get; }

    /// <summary>
    /// Gets the title of the widget.
    /// </summary>
    string Title { get; }

    /// <summary>
    /// Gets the order of the widget.
    /// </summary>
    int Order { get; }

    /// <summary>
    /// Gets the icon associated with the widget.
    /// </summary>
    MaterialIconKind Icon { get; }

    /// <summary>
    /// Initializes the Map Widget with a reference to a Map it belongs to.
    /// </summary>
    /// <param name="context">The map the widget is associated with.</param>
    /// <returns>The initialized MapWidget.</returns>
    IMapWidget Init(IMap context);
}