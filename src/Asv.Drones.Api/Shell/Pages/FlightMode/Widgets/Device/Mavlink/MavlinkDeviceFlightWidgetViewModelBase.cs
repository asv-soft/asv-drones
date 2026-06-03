using Asv.Avalonia;
using Asv.Avalonia.IO;
using Asv.Mavlink;
using Asv.Modeling;

namespace Asv.Drones.Api;

public abstract class MavlinkDeviceFlightWidgetViewModelBase<TMavlinkClientDevice, TSelf>(
    NavId id,
    TMavlinkClientDevice device,
    IDeviceManager deviceManager,
    IExtensionService ext
) : DeviceFlightWidgetViewModelBase<TMavlinkClientDevice, TSelf>(id, device, deviceManager, ext)
    where TSelf : class, IFlightWidget
    where TMavlinkClientDevice : MavlinkClientDevice
{
    public override int Order { get; } =
        device.Id is MavlinkClientDeviceId mavlinkId ? CreateOrderFromId(mavlinkId) : 0;

    private static int CreateOrderFromId(MavlinkClientDeviceId id)
    {
        return (id.Id.Target.SystemId * 1000) + id.Id.Target.ComponentId;
    }
}
