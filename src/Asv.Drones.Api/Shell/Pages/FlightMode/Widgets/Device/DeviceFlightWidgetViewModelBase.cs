using Asv.Avalonia;
using Asv.Avalonia.IO;
using Asv.Common;
using Asv.IO;
using Asv.Modeling;
using R3;

namespace Asv.Drones.Api;

public abstract class DeviceFlightWidgetViewModelBase<TDeviceContext, TSelf>
    : FlightWidgetViewModel<TSelf>
    where TDeviceContext : class, IClientDevice
    where TSelf : class, IFlightWidget
{
    protected DeviceFlightWidgetViewModelBase(
        NavId id,
        TDeviceContext device,
        IDeviceManager deviceManager,
        IExtensionService ext
    )
        : base(id, ext)
    {
        ArgumentNullException.ThrowIfNull(deviceManager);

        Position = WorkspaceDock.Left;
        Device = device;
        Header = device.Id.ToString();
        Icon = deviceManager.GetIcon(device.Id);
        IconColor = deviceManager.GetDeviceColor(device.Id);

        device
            .Name.ObserveOnUIThreadDispatcher()
            .Subscribe(x => Header = x)
            .DisposeItWith(Disposable);
    }

    public TDeviceContext Device { get; }
}
