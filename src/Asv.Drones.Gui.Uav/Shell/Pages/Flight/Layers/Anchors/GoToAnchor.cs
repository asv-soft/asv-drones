using System.Reactive.Disposables;
using System.Reactive.Linq;
using Asv.Avalonia.Map;
using Asv.Drones.Gui.Core;
using Asv.Mavlink;
using Asv.Mavlink.Vehicle;
using Avalonia.Media;
using Material.Icons;
using ReactiveUI;

namespace Asv.Drones.Gui.Uav
{
    public class GoToAnchor : MapAnchorBase
    {
        public GoToAnchor(IVehicle vehicle) : base(new(UavWellKnownUri.UavAnchorsBaseUri + $"/{vehicle.FullId}/goto"))
        {
            Size = 32;
            OffsetX = OffsetXEnum.Center;
            OffsetY = OffsetYEnum.Center;
            Icon = MaterialIconKind.Target;
            IconBrush = Brushes.LightSeaGreen;
            IsVisible = false;

            vehicle.GoToTarget.Select(_ => _.HasValue).DistinctUntilChanged().Subscribe(_ => IsVisible = _).DisposeWith(Disposable);
            vehicle.GoToTarget.Where(_=>_.HasValue).Subscribe(_ => Location = _.Value).DisposeWith(Disposable);

            vehicle.GoToTarget.Where(_ => _.HasValue)
                .Sample(TimeSpan.FromSeconds(1), RxApp.MainThreadScheduler)
                .Subscribe(_ => Description = TryCalculatteLeftTime(vehicle))
                .DisposeWith(Disposable);
        }

        private string TryCalculatteLeftTime(IVehicle vehicle)
        {
            var distance = 0.0;
            if (vehicle.GoToTarget.Value != null)
            {
                distance = vehicle.GlobalPosition.Value.DistanceTo(vehicle.GoToTarget.Value.Value);
            }

            if (vehicle.GpsGroundVelocity.Value != 0)
            {
                var leftTime = TimeSpan.FromSeconds(distance / vehicle.GpsGroundVelocity.Value);
                return $"Next target of     {vehicle.Name.Value}\n" +
                       $"Distance to target {distance:F0} m\n" +
                       $"Ground speed       {vehicle.GpsGroundVelocity.Value:F0} m/s\n" +
                       $"Left time ~        {(int)leftTime.TotalMinutes:00}:{leftTime.Seconds:00}";
            }
            else
            {
                return $"Next target of {vehicle.Name.Value}";
            }
        }
    }
}