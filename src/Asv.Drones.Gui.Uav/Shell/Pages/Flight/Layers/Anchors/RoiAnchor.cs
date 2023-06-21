﻿using System.Reactive.Disposables;
using System.Reactive.Linq;
using Asv.Avalonia.Map;
using Asv.Mavlink;
using Avalonia.Media;
using Material.Icons;
using ReactiveUI;

namespace Asv.Drones.Gui.Uav
{
    public class RoiAnchor : FlightAnchorBase
    {
        public RoiAnchor(IVehicleClient vehicle) : base(vehicle, "roi")
        {
            Size = 32;
            OffsetX = OffsetXEnum.Center;
            OffsetY = OffsetYEnum.Center;
            Icon = MaterialIconKind.ImageFilterCenterFocus;
            IconBrush = Brushes.LightSeaGreen;
            IsVisible = false;

            vehicle.Position.Roi.Select(_ => _.HasValue).DistinctUntilChanged().Subscribe(_ => IsVisible = _).DisposeWith(Disposable);
            vehicle.Position.Roi.Where(_ => _.HasValue).Subscribe(_ => Location = _.Value).DisposeWith(Disposable);

            vehicle.Name.Subscribe(_ => Title = $"{RS.RoiAnchor_Vehicle_Name} {_}").DisposeWith(Disposable);

            vehicle.Position.Yaw.Sample(TimeSpan.FromSeconds(1), RxApp.MainThreadScheduler).Subscribe(_ => Description = $"{RS.RoiAnchor_Vehicle_Yaw_Sample_Description}: {_:F0} {RS.RoiAnchor_Vehicle_Yaw_Sample_Unit}");
        }

    }
}