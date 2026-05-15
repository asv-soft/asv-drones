using Asv.Avalonia;
using Asv.Avalonia.IO;
using Asv.Mavlink;
using Asv.Modeling;
using Microsoft.Extensions.Logging;

namespace Asv.Drones.Api;

public class DroneFlightWidgetViewModelBase<TDrone, TSelf>(
    NavId id,
    IDeviceManager deviceManager,
    ILoggerFactory loggerFactory,
    IExtensionService ext
) : MavlinkDeviceFlightWidgetViewModelBase<TDrone, TSelf>(id, deviceManager, loggerFactory, ext)
    where TSelf : class, IFlightWidget<TDrone>
    where TDrone : MavlinkClientDevice { }
