using Asv.Avalonia;
using Asv.Avalonia.IO;
using Asv.Common;
using Asv.Drones.Api;
using Asv.IO;
using Asv.Mavlink;
using Asv.Modeling;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Drones;

public class FlightModeAnchorsExtension(IDeviceManager conn, ILoggerFactory loggerFactory)
    : IExtensionFor<IFlightModePage>
{
    public void Extend(IFlightModePage context, CompositeDisposable contextDispose)
    {
        conn.Explorer.InitializedDevices.PopulateTo(
                context.Map.Anchors,
                TryCreateAnchor,
                RemoveAnchor
            )
            .DisposeItWith(contextDispose);
    }

    private UavAnchor? TryCreateAnchor(IClientDevice device)
    {
        var pos = device.GetMicroservice<IPositionClientEx>();
        return pos != null ? new UavAnchor(device.Id, conn, device, pos, loggerFactory) : null;
    }

    private static bool RemoveAnchor(IClientDevice dev, UavAnchor anchor)
    {
        return anchor.DeviceId == dev.Id;
    }
}
