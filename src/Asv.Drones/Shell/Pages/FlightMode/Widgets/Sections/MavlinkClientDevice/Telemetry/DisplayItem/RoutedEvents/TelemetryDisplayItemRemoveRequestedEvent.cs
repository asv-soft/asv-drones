using System.Threading;
using Asv.Avalonia;
using Asv.Common;
using Asv.Modeling;

namespace Asv.Drones;

public sealed class TelemetryDisplayItemRemoveRequestedEvent(
    TelemetryDisplayItemViewModel source,
    CancellationToken cancel = default
) : AsyncRoutedEvent<IRoutable>(source, RoutingStrategy.Bubble)
{
    public string ItemId => source.Item.ItemId;
    public CancellationToken Cancel { get; } = cancel;
}
