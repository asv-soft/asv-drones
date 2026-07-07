using Asv.Drones.Api;
using Asv.IO;
using Asv.Mavlink;
using Microsoft.Extensions.DependencyInjection;

namespace Asv.Drones;

public class DroneWidgetCreationHandler(IServiceProvider services)
    : IClientDeviceWidgetCreationHandler
{
    public Type DeviceType => typeof(MavlinkClientDevice);

    public IFlightWidget? Create(in IClientDevice device)
    {
        if (device.GetMicroservice<IControlClient>() is not null)
        {
            if (device is not MavlinkClientDevice mavlinkDevice)
            {
                return null;
            }

            var widget = ActivatorUtilities.CreateInstance<DroneFlightWidgetViewModel>(
                services,
                mavlinkDevice
            );

            return widget;
        }

        return null;
    }
}
