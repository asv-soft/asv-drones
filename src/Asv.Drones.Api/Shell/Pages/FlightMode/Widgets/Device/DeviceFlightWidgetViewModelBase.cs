using Asv.Avalonia;
using Asv.Avalonia.IO;
using Asv.Common;
using Asv.IO;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Drones.Api;

public abstract class DeviceFlightWidgetViewModelBase<TDeviceContext, TSelf>
    : FlightWidgetViewModel<TDeviceContext, TSelf>
    where TDeviceContext : class, IClientDevice
    where TSelf : class, IFlightWidget<TDeviceContext>
{
    private readonly IDeviceManager _deviceManager;

    protected DeviceFlightWidgetViewModelBase(
        NavigationId id,
        IDeviceManager deviceManager,
        ILoggerFactory loggerFactory,
        IExtensionService ext
    )
        : base(id, loggerFactory, ext)
    {
        ArgumentNullException.ThrowIfNull(deviceManager);
        _deviceManager = deviceManager;
        Position = WorkspaceDock.Left;
    }

    public TDeviceContext? Device { get; private set; }

    public override void InitWith(TDeviceContext device)
    {
        ArgumentNullException.ThrowIfNull(device);
        InitArgs(device.Id.AsString());
        Device = device;
        Header = device.Id.ToString();
        Icon = _deviceManager.GetIcon(device.Id);
        IconColor = _deviceManager.GetDeviceColor(device.Id);
        device
            .Name.ObserveOnUIThreadDispatcher()
            .Subscribe(x => Header = x)
            .DisposeItWith(Disposable);
    }
}
