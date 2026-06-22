using System.Threading;
using Asv.Avalonia;
using Asv.Modeling;

namespace Asv.Drones;

public sealed class TelemetryDisplayItemRemoveRequestedEvent(
    TelemetryDisplayItemViewModel source,
    CancellationToken cancel = default
) : AsyncRoutedEvent<IViewModel>(source, RoutingStrategy.Bubble)
{
    public string ItemId => source.Item.Id.ToString();
    public CancellationToken Cancel { get; } = cancel;
}
