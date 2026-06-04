using Asv.Avalonia;
using Asv.Modeling;

namespace Asv.Drones;

/// <summary>
/// Represents an event triggered when a param item changes.
/// </summary>
public sealed class ParamItemChangedEvent(IViewModel source, object trackedObject)
    : AsyncRoutedEvent<IViewModel>(source, RoutingStrategy.Bubble)
{
    public object TrackedObject => trackedObject;
}
