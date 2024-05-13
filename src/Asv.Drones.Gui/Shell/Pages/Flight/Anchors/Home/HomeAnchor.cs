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
    public class HomeAnchor : FlightAnchorBase
    {
        public HomeAnchor(IVehicleClient vehicle) : base(vehicle, "home")
        {
            Size = 32;
            OffsetX = OffsetXEnum.Center;
            OffsetY = OffsetYEnum.Bottom;
            Icon = MaterialIconKind.HomeMapMarker;
            IconBrush = Brushes.LightSeaGreen;
            IsVisible = false;
            IsEditable = false;

            vehicle.Position.Home.Select(_ => _.HasValue).DistinctUntilChanged().Subscribe(_ => IsVisible = _)
                .DisposeItWith(Disposable);
            vehicle.Position.Home.Where(_ => _.HasValue).Subscribe(_ => Location = _.Value).DisposeItWith(Disposable);

            vehicle.Position.HomeDistance
                .Sample(TimeSpan.FromSeconds(1), RxApp.MainThreadScheduler)
                .Subscribe(_ =>
                {
                    // TODO: User Localize
                    // TODO: User IlocalizationService for speed, time and distance units 
                    Description = $"Launch of       {vehicle.Name.Value}\n" +
                                  $"Distance to UAV {vehicle.Position.HomeDistance.Value:F0} m";
                })
                .DisposeItWith(Disposable);
        }
    }
}