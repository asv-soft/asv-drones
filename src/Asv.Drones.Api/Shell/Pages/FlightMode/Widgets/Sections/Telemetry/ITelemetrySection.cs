using Asv.Mavlink;
using ObservableCollections;

namespace Asv.Drones.Api;

public sealed class TelemetrySectionConfig
{
    public string[]? ItemIds { get; set; }
}

#pragma warning disable SA1313
public sealed record TelemetrySectionArgs(
    MavlinkClientDevice? Device,
    IReadOnlyList<string> DefaultItemIds
);
#pragma warning restore SA1313

public interface ITelemetrySection : IFlightWidgetSection
{
    ObservableList<ITelemetryItem> Items { get; }
    bool TryAddItem(string itemId);
    bool TryRemoveItem(string itemId);
    bool TrySetItems(IReadOnlyList<string> itemIds);
    bool TryResetItems();
}
