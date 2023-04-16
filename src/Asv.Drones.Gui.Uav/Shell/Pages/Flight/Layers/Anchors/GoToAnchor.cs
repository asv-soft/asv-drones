using System.Reactive.Disposables;
using System.Reactive.Linq;
using Asv.Avalonia.Map;
using Asv.Common;
using Asv.Drones.Gui.Core;
using Asv.Mavlink;
using Asv.Mavlink.Vehicle;
using Avalonia.Media;
using Material.Icons;
using ReactiveUI;

namespace Asv.Drones.Gui.Uav
{
    public class GoToAnchor : FlightAnchorBase
    {
        public GoToAnchor(IVehicleClient vehicle) 
            : base(vehicle,"goto")
        {
            Size = 32;
            OffsetX = OffsetXEnum.Center;
            OffsetY = OffsetYEnum.Center;
            Icon = MaterialIconKind.Target;
            IconBrush = Brushes.LightSeaGreen;
            IsVisible = false;

            vehicle.Position.Target.Select(_ => _.HasValue).DistinctUntilChanged().Subscribe(_ => IsVisible = _).DisposeWith(Disposable);
            vehicle.Position.Target.Where(_=>_.HasValue).Subscribe(_ => Location = _.Value).DisposeWith(Disposable);

            vehicle.Position.Target.Where(_ => _.HasValue)
                .Sample(TimeSpan.FromSeconds(1), RxApp.MainThreadScheduler)
                .Subscribe(_ => Description = TryCalculatteLeftTime(vehicle))
                .DisposeWith(Disposable);
        }

        private string TryCalculatteLeftTime(IVehicleClient vehicle)
        {
            var distance = 0.0;
            if (vehicle.Position.Target.Value != null)
            {
                distance = vehicle.Position.Current.Value.DistanceTo(vehicle.Position.Target.Value.Value);
            }
            
            if (vehicle.Gnss.Main.GroundVelocity.Value != 0)
            {
                // TODO: User Localize
                // TODO: User IlocalizationService for speed, time and distance units 
                var leftTime = TimeSpan.FromSeconds(distance / vehicle.Gnss.Main.GroundVelocity.Value);
                return $"Next target of     {vehicle.Name.Value}\n" +
                       $"Distance to target {distance:F0} m\n" +
                       $"Ground speed       {vehicle.Gnss.Main.GroundVelocity.Value:F0} m/s\n" +
                       $"Left time ~        {(int)leftTime.TotalMinutes:00}:{leftTime.Seconds:00}";
            }
            else
            {
                return $"Next target of {vehicle.Name.Value}";
            }
        }
    }
}