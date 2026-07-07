using Asv.Avalonia;
using Asv.Modeling;

namespace Asv.Drones;

public sealed class TelemetryDisplayItemAddRequestedEvent(
    AddTelemetryDisplayItemViewModel source,
    CancellationToken cancel = default
) : AsyncRoutedEvent<IViewModel>(source, RoutingStrategy.Bubble)
{
    public CancellationToken Cancel { get; } = cancel;
}
