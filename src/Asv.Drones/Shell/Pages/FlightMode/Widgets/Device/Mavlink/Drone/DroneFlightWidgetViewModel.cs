using Asv.Avalonia;
using Asv.Avalonia.IO;
using Asv.Drones.Api;
using Asv.Mavlink;
using Asv.Modeling;

namespace Asv.Drones;

public class DroneFlightWidgetViewModel
    : DroneFlightWidgetViewModelBase<MavlinkClientDevice, IDroneFlightWidget>,
        IDroneFlightWidget
{
    public const string WidgetId = "drone";

    public DroneFlightWidgetViewModel(
        MavlinkClientDevice device,
        IDeviceManager deviceManager,
        IExtensionService ext
    )
        : this(
            new NavId(WidgetId, DevicePageViewModelMixin.CreateOpenPageArgs(device.Id)),
            device,
            deviceManager,
            ext
        ) { }

    private DroneFlightWidgetViewModel(
        NavId id,
        MavlinkClientDevice device,
        IDeviceManager deviceManager,
        IExtensionService ext
    )
        : base(id, device, deviceManager, ext) { }

    protected override void AfterLoadExtensions()
    {
        // nothing to do
    }
}
