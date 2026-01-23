using Asv.Avalonia;
using Asv.Common;

namespace Asv.Drones;

/// <summary>
/// Represents an event triggered when param item changes.
/// </summary>
/// <param name="source">.</param>
public sealed class ParamItemChangedEvent(IRoutable source, object trackedObject)
    : AsyncRoutedEvent<IRoutable>(source, RoutingStrategy.Bubble)
{
    public object TrackedObject => trackedObject;
}
