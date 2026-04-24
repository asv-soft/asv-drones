using Asv.Mavlink;

namespace Asv.Drones.Api;

public interface IMavlinkDeviceFlightWidget : IMavlinkDeviceFlightWidget<MavlinkClientDevice> { }

public interface IMavlinkDeviceFlightWidget<TMavlinkClientDevice>
    : IDeviceFlightWidget<TMavlinkClientDevice>
    where TMavlinkClientDevice : MavlinkClientDevice { }
