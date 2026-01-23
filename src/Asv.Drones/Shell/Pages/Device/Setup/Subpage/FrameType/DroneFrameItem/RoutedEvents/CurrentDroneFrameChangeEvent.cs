using Asv.Avalonia;
using Asv.Common;

namespace Asv.Drones;

/// <summary>
/// Represents an event triggered when the current drone frame item changes.
/// </summary>
/// <param name="source">.</param>
public sealed class CurrentDroneFrameChangeEvent(DroneFrameItemViewModel source)
    : AsyncRoutedEvent<IRoutable>(source, RoutingStrategy.Bubble) { }
