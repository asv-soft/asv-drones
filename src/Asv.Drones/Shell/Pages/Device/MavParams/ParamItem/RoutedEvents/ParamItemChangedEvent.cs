using Asv.Avalonia;
using Asv.Common;
using Asv.Modeling;

namespace Asv.Drones;

/// <summary>
/// Represents an event triggered when param item changes.
/// </summary>
/// <param name="source">.</param>
public sealed class ParamItemChangedEvent(IViewModel source, object trackedObject)
    : AsyncRoutedEvent<IViewModel>(source, RoutingStrategy.Bubble)
{
    public object TrackedObject => trackedObject;
}
