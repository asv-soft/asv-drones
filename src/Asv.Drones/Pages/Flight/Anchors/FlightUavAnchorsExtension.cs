using System.Composition;
using Asv.Avalonia;
using Asv.Avalonia.IO;
using Asv.Common;
using Asv.Drones.Api;
using Asv.IO;
using Asv.Mavlink;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Drones;

[ExportExtensionFor<IFlightMode>]
[method: ImportingConstructor]
public class FlightUavAnchorsExtension(IDeviceManager conn, ILoggerFactory loggerFactory)
    : IExtensionFor<IFlightMode>
{
    public void Extend(IFlightMode context, CompositeDisposable contextDispose)
    {
        conn.Explorer.InitializedDevices.PopulateTo(context.Anchors, TryCreateAnchor, RemoveAnchor)
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
