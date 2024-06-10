using System;
using System.Reactive.Linq;
using Asv.Avalonia.Map;
using Asv.Common;
using Asv.Mavlink;
using Avalonia.Media;
using Material.Icons;
using ReactiveUI;

namespace Asv.Drones.Gui
{
    public class RoiAnchor : FlightAnchorBase
    {
        public RoiAnchor(IVehicleClient vehicle) : base(vehicle, "roi")
        {
            Size = 32;
            BaseSize = 32;
            OffsetX = OffsetXEnum.Center;
            OffsetY = OffsetYEnum.Center;
            Icon = MaterialIconKind.ImageFilterCenterFocus;
            IconBrush = Brushes.LightSeaGreen;
            IsVisible = false;

            vehicle.Position.Roi.Select(_ => _.HasValue).DistinctUntilChanged().Subscribe(_ => IsVisible = _)
                .DisposeItWith(Disposable);
            vehicle.Position.Roi.Where(_ => _.HasValue).Subscribe(_ => Location = _.Value).DisposeItWith(Disposable);

            vehicle.Name.Subscribe(_ => Title = $"{RS.RoiAnchor_Vehicle_Name} {_}").DisposeItWith(Disposable);

            vehicle.Position.Yaw.Sample(TimeSpan.FromSeconds(1), RxApp.MainThreadScheduler).Subscribe(_ =>
                Description =
                    $"{RS.RoiAnchor_Vehicle_Yaw_Sample_Description}: {_:F0} {RS.RoiAnchor_Vehicle_Yaw_Sample_Unit}");
        }
    }
}