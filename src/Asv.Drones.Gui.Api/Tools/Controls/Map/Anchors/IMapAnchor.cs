using Asv.Avalonia.Map;

namespace Asv.Drones.Gui.Api;

/// <summary>
/// Provides an interface that describes the structure and behavior of a Map Anchor.
/// Map Anchor represents a point of interest on the map.
/// </summary>
public interface IMapAnchor : IMapAnchorViewModel, IViewModel
{
    /// <summary>
    /// Initializes the MapAnchor with a reference to a Map it belongs to.
    /// </summary>
    /// <param name="map">The map the anchor is associated with.</param>
    /// <returns>The initialized MapAnchor.</returns>
    IMapAnchor Init(IMap map);
}