using System;
using Asv.Drones.Api;
using Asv.IO;
using Asv.Mavlink;
using Microsoft.Extensions.DependencyInjection;

namespace Asv.Drones.Plane;

public class PlaneWidgetCreationHandler(IServiceProvider services)
    : IClientDeviceWidgetCreationHandler
{
    public Type DeviceType => typeof(ArduPlaneClientDevice);

    public IFlightWidget? Create(in IClientDevice device)
    {
        if (device.GetMicroservice<IControlClient>() is not null)
        {
            if (device is not ArduPlaneClientDevice plane)
            {
                return null;
            }

            var widget = services.GetService<IPlaneWidget>();

            widget?.InitWith(plane);

            return widget;
        }

        return null;
    }
}
