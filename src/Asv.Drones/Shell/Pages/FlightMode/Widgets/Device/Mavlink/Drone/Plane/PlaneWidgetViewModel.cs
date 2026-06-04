using Asv.Avalonia;
using Asv.Avalonia.IO;
using Asv.Drones.Api;
using Asv.Mavlink;
using Asv.Modeling;

namespace Asv.Drones.Plane;

public interface IPlaneWidget : IPlaneWidget<ArduPlaneClientDevice> { }

public interface IPlaneWidget<TPlane> : IDroneFlightWidget<TPlane>
    where TPlane : ArduPlaneClientDevice { }

public class PlaneWidgetViewModel
    : PlaneWidgetViewModelBase<ArduPlaneClientDevice, IPlaneWidget>,
        IPlaneWidget
{
    public const string WidgetId = "plane";

    public PlaneWidgetViewModel(
        ArduPlaneClientDevice device,
        IDeviceManager deviceManager,
        IExtensionService ext
    )
        : base(
            new NavId(WidgetId, DevicePageViewModelMixin.CreateOpenPageArgs(device.Id)),
            device,
            deviceManager,
            ext
        ) { }
}

public abstract class PlaneWidgetViewModelBase<TPlane, TSelf>
    : DroneFlightWidgetViewModelBase<TPlane, TSelf>
    where TSelf : class, IFlightWidget
    where TPlane : ArduPlaneClientDevice
{
    protected PlaneWidgetViewModelBase(
        NavId id,
        TPlane device,
        IDeviceManager deviceManager,
        IExtensionService ext
    )
        : base(id, device, deviceManager, ext) { }
}
