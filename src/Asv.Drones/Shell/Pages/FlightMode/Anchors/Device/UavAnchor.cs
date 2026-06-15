using System.Collections.Generic;
using Asv.Avalonia;
using Asv.Avalonia.IO;
using Asv.Common;
using Asv.IO;
using Asv.Mavlink;
using R3;

namespace Asv.Drones;

public class UavAnchor : DeviceAnchor<UavAnchor>
{
    public const string UavAnchorIdBase = "uav";

    public UavAnchor()
    {
        DesignTime.ThrowIfNotDesignMode();
    }

    public UavAnchor(IDeviceManager mng, IClientDevice dev, IExtensionService ext)
        : base(UavAnchorIdBase, [], mng, dev, ext)
    {
        IsReadOnly = true;
        IsVisible = true;
        UseMapRotation = true;

        var pos = dev.GetRequiredMicroservice<IPositionClientEx>();

        pos.Current.Skip(1)
            .DistinctUntilChanged()
            .ThrottleLast(TimeSpan.FromMilliseconds(200))
            .ObserveOnUIThreadDispatcher()
            .Subscribe(x => Location = x)
            .DisposeItWith(Disposable);

        pos.Yaw.Skip(1)
            .DistinctUntilChanged()
            .ThrottleLast(TimeSpan.FromMilliseconds(200))
            .ObserveOnUIThreadDispatcher()
            .Subscribe(x => Azimuth = x)
            .DisposeItWith(Disposable);

        this.DrawLine(
                pos.Current.Skip(1)
                    .DistinctUntilChanged()
                    .ThrottleLast(TimeSpan.FromMilliseconds(200))
                    .ObserveOnUIThreadDispatcher(),
                pos.Home.Skip(1)
                    .Where(x => x.HasValue)
                    .DistinctUntilChanged()
                    .ThrottleLast(TimeSpan.FromMilliseconds(200))
                    .Cast<GeoPoint?, GeoPoint>()
                    .ObserveOnUIThreadDispatcher()
            )
            .DisposeItWith(Disposable);
    }
}
