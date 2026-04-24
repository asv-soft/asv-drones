using Asv.Avalonia;
using Asv.Avalonia.IO;
using Asv.Mavlink;
using Microsoft.Extensions.Logging;

namespace Asv.Drones.Api;

public abstract class MavlinkDeviceFlightWidgetViewModelBase<TMavlinkClientDevice, TSelf>(
    NavigationId id,
    IDeviceManager deviceManager,
    ILoggerFactory loggerFactory,
    IExtensionService ext
)
    : DeviceFlightWidgetViewModelBase<TMavlinkClientDevice, TSelf>(
        id,
        deviceManager,
        loggerFactory,
        ext
    )
    where TSelf : class, IFlightWidget<TMavlinkClientDevice>
    where TMavlinkClientDevice : MavlinkClientDevice
{
    private int _order;
    public override int Order => _order;

    public override void InitWith(TMavlinkClientDevice device)
    {
        base.InitWith(device);

        var mavlinkId =
            device.Id as MavlinkClientDeviceId
            ?? throw new Exception($"Should be {typeof(MavlinkClientDeviceId)}");

        _order = CreateOrderFromId(mavlinkId);
    }

    private static int CreateOrderFromId(MavlinkClientDeviceId id)
    {
        return (id.Id.Target.SystemId * 1000) + id.Id.Target.ComponentId;
    }
}
