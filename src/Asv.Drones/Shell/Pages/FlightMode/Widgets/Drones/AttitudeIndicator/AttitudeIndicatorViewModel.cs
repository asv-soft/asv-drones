using System;
using System.Collections.Generic;
using Asv.Avalonia;
using Asv.Avalonia.IO;
using Asv.Common;
using Asv.IO;
using Asv.Mavlink;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Drones;

public class AttitudeIndicatorViewModel : RoutableViewModel, IDashboardWidget
{
    private readonly IDeviceManager _deviceManager;
    public const string WidgetId = "alt-indicator-widget-dashboard-item";

    public AttitudeIndicatorViewModel(IDeviceManager deviceManager, ILoggerFactory loggerFactory)
        : base(WidgetId, loggerFactory)
    {
        _deviceManager = deviceManager;
    }

    public BindableReactiveProperty<double> Roll { get; private set; }
    public BindableReactiveProperty<double> Pitch { get; private set; }
    public DeviceId DeviceId { get; private set; }

    public void Attach(DeviceId deviceId)
    {
        var device = _deviceManager.Explorer.Devices[deviceId];
        var positionClientEx =
            device.GetMicroservice<IPositionClientEx>()
            ?? throw new ArgumentException(
                $"Unable to load {nameof(PositionClientEx)} from {deviceId}"
            );

        Roll = positionClientEx.Roll.ToBindableReactiveProperty().DisposeItWith(Disposable);
        Pitch = positionClientEx.Pitch.ToBindableReactiveProperty().DisposeItWith(Disposable);
    }

    public override IEnumerable<IRoutable> GetChildren()
    {
        return [];
    }
}
