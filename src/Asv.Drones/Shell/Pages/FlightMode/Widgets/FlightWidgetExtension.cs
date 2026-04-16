using System;
using Asv.Avalonia;
using Asv.Avalonia.IO;
using Asv.Common;
using Asv.IO;
using Asv.Mavlink;
using Asv.Modeling;
using Microsoft.Extensions.DependencyInjection;
using R3;

namespace Asv.Drones;

public class FlightWidgetExtension(IDeviceManager conn, IServiceProvider containerHost)
    : IExtensionFor<IFlightModePage>
{
    public void Extend(IFlightModePage context, CompositeDisposable contextDispose)
    {
        conn.Explorer.InitializedDevices.PopulateTo(context.Widgets, TryCreateWidget, RemoveWidget)
            .DisposeItWith(contextDispose);
    }

    private IFlightWidget? TryCreateWidget(IClientDevice device)
    {
        var isInit = device.State.CurrentValue == ClientDeviceState.Complete;

        if (!isInit)
        {
            return null;
        }

        if (device.GetMicroservice<ArduCopterControlClient>() is not null)
        {
            var mavlinkClientDeviceId = device.Id as MavlinkClientDeviceId;

            var widget = containerHost.GetService<IDroneFlightWidget>();

            widget?.Attach(device.Id);

            return widget;
        }

        return null;
    }

    private bool RemoveWidget(IClientDevice model, IFlightWidget vm)
    {
        return model.Id == vm.DeviceId;
    }
}
