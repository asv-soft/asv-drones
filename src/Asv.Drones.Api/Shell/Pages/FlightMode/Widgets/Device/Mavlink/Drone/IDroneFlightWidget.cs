using Asv.Mavlink;

namespace Asv.Drones.Api;

public interface IDroneFlightWidget : IDroneFlightWidget<MavlinkClientDevice> { }

public interface IDroneFlightWidget<TDrone>
    : IMavlinkDeviceFlightWidget<TDrone>,
        IDroneActionTarget<TDrone>
    where TDrone : MavlinkClientDevice { }
