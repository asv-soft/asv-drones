using Asv.Avalonia;
using Asv.Avalonia.IO;
using Asv.Drones.Api;
using Asv.Mavlink;
using Microsoft.Extensions.Logging;

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
        IDeviceManager deviceManager,
        ILoggerFactory loggerFactory,
        IExtensionService ext
    )
        : base(WidgetId, deviceManager, loggerFactory, ext) { }
}

public abstract class PlaneWidgetViewModelBase<TPlane, TSelf>
    : DroneFlightWidgetViewModelBase<TPlane, TSelf>
    where TSelf : class, IFlightWidget<ArduPlaneClientDevice>
    where TPlane : ArduPlaneClientDevice
{
    protected PlaneWidgetViewModelBase(
        NavigationId id,
        IDeviceManager deviceManager,
        ILoggerFactory loggerFactory,
        IExtensionService ext
    )
        : base(id, deviceManager, loggerFactory, ext) { }
}
