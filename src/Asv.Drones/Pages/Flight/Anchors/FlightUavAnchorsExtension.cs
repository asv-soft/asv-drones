using System.Collections.Generic;
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
public class FlightUavAnchorsExtension(
    IDeviceManager conn,
    INavigationService navigationService,
    IUnitService unitService,
    ILoggerFactory loggerFactory
) : IExtensionFor<IFlightMode>
{
    public void Extend(IFlightMode context, CompositeDisposable contextDispose)
    {
        conn.Explorer.Devices.PopulateTo(context.Anchors, TryCreateAnchor, RemoveAnchor)
            .DisposeItWith(contextDispose);
        conn.Explorer.Devices.PopulateTo(context.Widgets, TryCreateWidget, RemoveWidget)
            .DisposeItWith(contextDispose);
    }

    private UavWidgetViewModel? TryCreateWidget(KeyValuePair<DeviceId, IClientDevice> device)
    {
        var pos = device.Value.GetMicroservice<IPositionClientEx>();
        return pos != null
            ? new UavWidgetViewModel(device.Value,navigationService, unitService, conn, loggerFactory)
            : null;
    }

    private bool RemoveWidget(KeyValuePair<DeviceId, IClientDevice> model, UavWidgetViewModel vm)
    {
        return model.Key == vm.Device.Id;
    }

    private UavAnchor? TryCreateAnchor(KeyValuePair<DeviceId, IClientDevice> device)
    {
        var pos = device.Value.GetMicroservice<IPositionClientEx>();
        return pos != null ? new UavAnchor(device.Key, conn, device.Value, pos) : null;
    }

    private static bool RemoveAnchor(KeyValuePair<DeviceId, IClientDevice> dev, UavAnchor anchor)
    {
        return anchor.DeviceId == dev.Key;
    }
}
