using Asv.Avalonia;
using Asv.Avalonia.IO;
using Asv.Mavlink;
using Asv.Modeling;

namespace Asv.Drones.Api;

public class DroneFlightWidgetViewModelBase<TDrone, TSelf>(
    NavId id,
    TDrone device,
    IDeviceManager deviceManager,
    IExtensionService ext
) : MavlinkDeviceFlightWidgetViewModelBase<TDrone, TSelf>(id, device, deviceManager, ext)
    where TSelf : class, IFlightWidget
    where TDrone : MavlinkClientDevice { }
