using System;
using Asv.Avalonia;
using Asv.Avalonia.Map;
using Asv.Common;
using Asv.IO;
using Asv.Mavlink;
using Material.Icons;
using R3;

namespace Asv.Drones;

public class UavAnchor : MapAnchor<UavAnchor>
{
    private const uint CurrentUavPositionChangeThrottleMs = 200;
    public DeviceId DeviceId { get; }

    public UavAnchor()
        : base("uav_design_time")
    {
        DesignTime.ThrowIfNotDesignMode();
    }

    public UavAnchor(DeviceId deviceId, IClientDevice dev, IPositionClientEx pos)
        : base("uav")
    {
        DeviceId = deviceId;
        InitArgs(deviceId.AsString());
        IsReadOnly = true;
        IsVisible = true;
        Icon = DeviceIconMixin.GetIcon(deviceId) ?? MaterialIconKind.Memory;
        Foreground = DeviceIconMixin.GetIconBrush(deviceId);
        CenterX = DeviceIconMixin.GetIconCenterX(deviceId);
        CenterY = DeviceIconMixin.GetIconCenterY(deviceId);
        dev.Name.Subscribe(x => Title = x ?? string.Empty).DisposeItWith(Disposable);
        pos.Current.Subscribe(x => Location = x).DisposeItWith(Disposable);
        pos.Yaw.Subscribe(x => Azimuth = x).DisposeItWith(Disposable);

        var currentUavLocation = pos.Current.CurrentValue;
        var currentHomeLocation = pos.Home.CurrentValue ?? GeoPoint.Zero;
        pos.GetHomePosition().SafeFireAndForget();
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
