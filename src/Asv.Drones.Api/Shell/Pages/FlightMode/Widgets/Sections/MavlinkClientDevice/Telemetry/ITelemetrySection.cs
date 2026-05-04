using Asv.Mavlink;
using ObservableCollections;

namespace Asv.Drones.Api;

public interface ITelemetrySection : IFlightWidgetSection<MavlinkClientDevice>
{
    ObservableList<ITelemetryItem> Items { get; }
    bool TryAddItem(string itemId);
    bool TryRemoveItem(string itemId);
    bool TrySetItems(IReadOnlyList<string> itemIds);
    bool TryResetItems();
}
