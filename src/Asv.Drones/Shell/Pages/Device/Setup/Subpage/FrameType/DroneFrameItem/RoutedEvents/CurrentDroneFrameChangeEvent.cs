using Asv.Avalonia;
using Asv.Modeling;

namespace Asv.Drones;

/// <summary>
/// Represents an event triggered when the current drone frame item changes.
/// </summary>
/// <param name="source">.</param>
public sealed class CurrentDroneFrameChangeEvent(DroneFrameItemViewModel source)
    : AsyncRoutedEvent<IViewModel>(source, RoutingStrategy.Bubble) { }
