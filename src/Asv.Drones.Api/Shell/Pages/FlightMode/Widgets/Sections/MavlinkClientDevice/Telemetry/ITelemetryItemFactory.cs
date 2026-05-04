using Asv.IO;

namespace Asv.Drones.Api;

public interface ITelemetryItemFactory
{
    string ItemId { get; }
    string DisplayName { get; }

    bool CanCreate(in IClientDevice device);

    ITelemetryItem Create(in IClientDevice device);
    ITelemetryItem CreatePreview();

    ITelemetryItem? TryCreate(in IClientDevice device)
    {
        if (!CanCreate(device))
        {
            return null;
        }

        return Create(device);
    }
}
