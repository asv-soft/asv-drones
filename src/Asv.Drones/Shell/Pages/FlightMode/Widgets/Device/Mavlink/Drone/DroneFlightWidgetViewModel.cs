using Asv.Avalonia;
using Asv.Avalonia.IO;
using Asv.Drones.Api;
using Asv.Mavlink;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Asv.Drones;

public class DroneFlightWidgetViewModel(
    IDeviceManager deviceManager,
    ILoggerFactory loggerFactory,
    IExtensionService ext
)
    : DroneFlightWidgetViewModelBase<MavlinkClientDevice, IDroneFlightWidget>(
        WidgetId,
        deviceManager,
        loggerFactory,
        ext
    ),
        IDroneFlightWidget
{
    public const string WidgetId = "drone";

    public DroneFlightWidgetViewModel()
        : this(
            NullDeviceManager.Instance,
            NullLoggerFactory.Instance,
            NullExtensionService.Instance
        ) { }

    protected override void AfterLoadExtensions()
    {
        // nothing to do
    }
}
