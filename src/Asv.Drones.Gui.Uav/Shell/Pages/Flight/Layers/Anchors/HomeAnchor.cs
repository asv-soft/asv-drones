using System.Reactive.Disposables;
using System.Reactive.Linq;
using Asv.Avalonia.Map;
using Asv.Common;
using Asv.Drones.Gui.Core;
using Asv.Mavlink;
using Avalonia.Media;
using Material.Icons;
using ReactiveUI;

namespace Asv.Drones.Gui.Uav
{
    public class HomeAnchor : FlightAnchorBase
    {
        public HomeAnchor(IVehicle vehicle): base(vehicle, "home")
        {
            Size = 32;
            OffsetX = OffsetXEnum.Center;
            OffsetY = OffsetYEnum.Bottom;
            Icon = MaterialIconKind.HomeMapMarker;
            IconBrush = Brushes.LightSeaGreen;
            IsVisible = false;

            vehicle.Home.Select(_ => _.HasValue).DistinctUntilChanged().ObserveOn(RxApp.MainThreadScheduler).Subscribe(_ => IsVisible = _).DisposeWith(Disposable);
            vehicle.Home.Where(_ => _.HasValue).ObserveOn(RxApp.MainThreadScheduler).Subscribe(_ => Location = _.Value).DisposeWith(Disposable);

            vehicle.HomeDistance
                .Sample(TimeSpan.FromSeconds(1), RxApp.MainThreadScheduler)
                .Where(_ => _.HasValue)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(_ =>
                {
                    // TODO: User Localize
                    // TODO: User IlocalizationService for speed, time and distance units 
                    Description = $"Launch of       {vehicle.Name.Value}\n" +
                                  $"Distance to UAV {vehicle.HomeDistance.Value:F0} m";
                })
                .DisposeWith(Disposable);

        }
    }
}