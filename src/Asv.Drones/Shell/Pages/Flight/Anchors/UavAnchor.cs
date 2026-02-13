using System;
using Asv.Avalonia;
using Asv.Avalonia.GeoMap;
using Asv.Avalonia.IO;
using Asv.Common;
using Asv.Drones.Api;
using Asv.IO;
using Asv.Mavlink;
using Material.Icons;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Drones;

public class UavAnchor : MapAnchor<UavAnchor>
{
    public const string UavAnchorIdBase = "uav";
    private const uint CurrentUavPositionChangeThrottleMs = 200;
    public DeviceId DeviceId { get; }

    public UavAnchor()
        : base(DesignTime.Id, DesignTime.LoggerFactory)
    {
        DesignTime.ThrowIfNotDesignMode();
    }

    public UavAnchor(
        DeviceId deviceId,
        IDeviceManager mng,
        IClientDevice dev,
        IPositionClientEx pos,
        ILoggerFactory loggerFactory
    )
        : base(UavAnchorIdBase, loggerFactory)
    {
        DeviceId = deviceId;
        InitArgs(deviceId.AsString());
        IsReadOnly = true;
        IsVisible = true;
        Icon = mng.GetIcon(deviceId) ?? MaterialIconKind.Memory;
        IconColor = mng.GetDeviceColor(deviceId);
        CenterX = DeviceIconMixin.GetIconCenterX(deviceId);
        CenterY = DeviceIconMixin.GetIconCenterY(deviceId);
        dev.Name.Subscribe(x => Title = x ?? string.Empty).DisposeItWith(Disposable);
        pos.Current.Subscribe(x => Location = x).DisposeItWith(Disposable);
        pos.Yaw.Subscribe(x => Azimuth = x).DisposeItWith(Disposable);
        var currentUavLocation = pos.Current.CurrentValue;
        var currentHomeLocation = pos.Home.CurrentValue ?? GeoPoint.Zero;
        pos.Home.Subscribe(x =>
            {
                Polygon.Remove(currentHomeLocation);
                if (x is null)
                {
                    return;
                }

                Polygon.Add(x.Value);
                currentHomeLocation = x.Value;
            })
            .DisposeItWith(Disposable);
        pos.Current.ThrottleLast(TimeSpan.FromMilliseconds(CurrentUavPositionChangeThrottleMs))
            .Subscribe(x =>
            {
                Polygon.Remove(currentUavLocation);
                Polygon.Add(x);
                currentUavLocation = x;
            })
            .DisposeItWith(Disposable);
    }
}
