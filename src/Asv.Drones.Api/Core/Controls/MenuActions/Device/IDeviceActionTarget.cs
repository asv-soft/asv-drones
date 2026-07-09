using Asv.IO;
using Asv.Mavlink;

namespace Asv.Drones.Api;

public interface IDeviceActionTarget<out TDeviceContext> : IMenuActionTarget
    where TDeviceContext : class, IClientDevice
{
    TDeviceContext Device { get; }
}

public interface IDeviceActionTarget : IDeviceActionTarget<IClientDevice> { }

public interface IMavlinkDeviceActionTarget<out TMavlinkClientDevice>
    : IDeviceActionTarget<TMavlinkClientDevice>
    where TMavlinkClientDevice : MavlinkClientDevice { }

public interface IMavlinkDeviceActionTarget : IMavlinkDeviceActionTarget<MavlinkClientDevice> { }

public interface IDroneActionTarget<out TDeviceContext> : IDeviceActionTarget<TDeviceContext>
    where TDeviceContext : class, IClientDevice { }

public interface IDroneActionTarget : IDroneActionTarget<IClientDevice> { }
