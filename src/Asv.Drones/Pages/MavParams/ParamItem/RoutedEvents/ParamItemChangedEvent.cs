using Asv.Avalonia;

namespace Asv.Drones;

/// <summary>
/// Represents an event triggered when param item changes.
/// </summary>
/// <param name="source">.</param>
public sealed class ParamItemChangedEvent(IRoutable source, object caller)
    : AsyncRoutedEvent(source, RoutingStrategy.Bubble)
{
    public object Caller => caller;
}
