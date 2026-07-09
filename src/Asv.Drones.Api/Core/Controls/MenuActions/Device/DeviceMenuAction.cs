using Asv.Avalonia;
using Asv.IO;
using Asv.Mavlink;
using R3;

namespace Asv.Drones.Api;

public abstract class DeviceMenuAction<TTarget, TDeviceContext> : MenuAction<TTarget>
    where TTarget : class, IDeviceActionTarget<TDeviceContext>
    where TDeviceContext : class, IClientDevice
{
    protected DeviceMenuAction(string id)
        : this("device-action.", id) { }

    protected DeviceMenuAction(string baseId, string id)
        : base(baseId, id) { }

    protected abstract override IMenuItem? TryCreateAction(
        TTarget target,
        CompositeDisposable contextDispose
    );
}

public abstract class DeviceMenuAction<TTarget> : DeviceMenuAction<TTarget, IClientDevice>
    where TTarget : class, IDeviceActionTarget<IClientDevice>
{
    protected DeviceMenuAction(string id)
        : base(id) { }

    protected DeviceMenuAction(string baseId, string id)
        : base(baseId, id) { }
}

public abstract class MavlinkDeviceMenuAction<TTarget, TMavlinkClientDevice>
    : DeviceMenuAction<TTarget, TMavlinkClientDevice>
    where TTarget : class, IMavlinkDeviceActionTarget<TMavlinkClientDevice>
    where TMavlinkClientDevice : MavlinkClientDevice
{
    protected MavlinkDeviceMenuAction(string id)
        : base("mavlink-device-action.", id) { }

    protected MavlinkDeviceMenuAction(string baseId, string id)
        : base(baseId, id) { }
}

public abstract class MavlinkDeviceMenuAction<TTarget>
    : MavlinkDeviceMenuAction<TTarget, MavlinkClientDevice>
    where TTarget : class, IMavlinkDeviceActionTarget<MavlinkClientDevice>
{
    protected MavlinkDeviceMenuAction(string id)
        : base(id) { }

    protected MavlinkDeviceMenuAction(string baseId, string id)
        : base(baseId, id) { }
}

public abstract class DroneMenuAction<TTarget, TDeviceContext>
    : DeviceMenuAction<TTarget, TDeviceContext>
    where TTarget : class, IDeviceActionTarget<TDeviceContext>
    where TDeviceContext : class, IClientDevice
{
    protected DroneMenuAction(string id)
        : base("drone-action.", id) { }
}

public abstract class DroneMenuAction<TTarget> : DroneMenuAction<TTarget, IClientDevice>
    where TTarget : class, IDeviceActionTarget<IClientDevice>
{
    protected DroneMenuAction(string id)
        : base(id) { }
}
