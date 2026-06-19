using Asv.Avalonia;
using Asv.IO;
using Asv.Mavlink;
using ObservableCollections;

namespace Asv.Drones.Api;

public interface ITelemetrySection : IFlightWidgetSection
{
    ObservableList<IRttBoxViewModel> Items { get; }
    bool TryAddItem(string itemId);
    bool TryRemoveItem(string itemId);
    bool TrySetItems(IReadOnlyList<string> itemIds);
    bool TryResetItems();
}

#pragma warning disable SA1313
public sealed record TelemetrySectionArgs(
    IClientDevice? Device,
    IReadOnlyList<string> DefaultItemIds
);
#pragma warning restore SA1313
