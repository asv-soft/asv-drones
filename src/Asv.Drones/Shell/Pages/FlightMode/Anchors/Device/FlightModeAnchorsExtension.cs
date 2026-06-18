using Asv.Avalonia;
using Asv.Avalonia.IO;
using Asv.Common;
using Asv.Drones.Api;
using Asv.IO;
using Asv.Mavlink;
using Asv.Modeling;
using Avalonia.Threading;
using R3;

namespace Asv.Drones;

public class FlightModeAnchorsExtension(IDeviceManager manager, IExtensionService ext)
    : IExtensionFor<IFlightModePage>
{
    public void Extend(IFlightModePage context, CompositeDisposable contextDispose)
    {
        manager
            .Explorer.InitializedDevices.PopulateTo(
                context.Map.Anchors,
                TryCreateAnchor,
                RemoveAnchor,
                synchronizationContext: new AvaloniaSynchronizationContext(
                    Dispatcher.UIThread,
                    DispatcherPriority.Default
                )
            )
            .DisposeItWith(contextDispose);
    }

    private UavAnchor? TryCreateAnchor(IClientDevice device)
    {
        var pos = device.GetMicroservice<IPositionClientEx>();
        return pos != null ? new UavAnchor(manager, device, ext) : null;
    }

    private static bool RemoveAnchor(IClientDevice dev, UavAnchor anchor)
    {
        return anchor.Device.Id == dev.Id;
    }
}
