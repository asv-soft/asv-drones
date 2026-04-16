using System;
using System.Linq;
using Asv.Avalonia;
using Asv.Avalonia.IO;
using Asv.Common;
using Asv.Drones.Api;
using Asv.IO;
using Asv.Mavlink;
using Asv.Modeling;
using Microsoft.Extensions.DependencyInjection;
using R3;

namespace Asv.Drones;

public class FlightModeDroneFlightWidgetExtension(IDeviceManager conn, IServiceProvider services)
    : IExtensionFor<IFlightModePage>
{
    public void Extend(IFlightModePage context, CompositeDisposable contextDispose)
    {
        conn.Explorer.InitializedDevices.PopulateTo(
                context.Widgets,
                TryCreateWidget,
                IsRemoveWidget
            )
            .DisposeItWith(contextDispose);
    }

    private IDroneFlightWidget? TryCreateWidget(IClientDevice device)
    {
        if (device.GetMicroservice<IControlClient>() is not null)
        {
            if (device is not MavlinkClientDevice mavlinkDevice)
            {
                return null;
            }

            var widget = services.GetService<IDroneFlightWidget>();

            widget?.InitWith(mavlinkDevice);

            return widget;
        }

        return null;
    }

    private static bool IsRemoveWidget(IClientDevice model, IDroneFlightWidget vm)
    {
        return model.Id == vm.Device?.Id;
    }
}
