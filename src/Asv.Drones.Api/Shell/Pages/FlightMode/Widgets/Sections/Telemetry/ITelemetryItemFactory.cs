using Asv.Avalonia;
using Asv.IO;

namespace Asv.Drones.Api;

public interface ITelemetryItemFactory
{
    string ItemId { get; }

    bool CanCreate(in IClientDevice device);

    IRttBoxViewModel Create(in IClientDevice device);
    IRttBoxViewModel CreatePreview();

    IRttBoxViewModel? TryCreate(in IClientDevice device)
    {
        if (!CanCreate(device))
        {
            return null;
        }

        return Create(device);
    }
}
