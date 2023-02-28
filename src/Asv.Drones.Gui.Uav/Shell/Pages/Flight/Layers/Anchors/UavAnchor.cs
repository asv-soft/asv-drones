using System.Reactive.Disposables;
using System.Reactive.Linq;
using Asv.Avalonia.Map;
using Asv.Drones.Gui.Core;
using Asv.Mavlink;
using Avalonia.Media;
using Material.Icons;

namespace Asv.Drones.Gui.Uav
{
    public class UavAnchor: MapAnchorBase
    {
        private readonly IVehicle _vehicle;

        public UavAnchor(IVehicle vehicle):base(new(UavWellKnownUri.UavAnchorsBaseUri+ $"/{vehicle.FullId}/uav"))
        {
            _vehicle = vehicle;
            Size = 48;
            OffsetX = OffsetXEnum.Center;
            OffsetY = OffsetYEnum.Center;
            IconBrush = Brushes.Red;
            IsVisible = true;
            UpdateIcon(vehicle.Class.Value);
            vehicle.GlobalPosition.Subscribe(_ => Location = _).DisposeWith(Disposable);
            vehicle.Yaw.Select(_ => Math.Round(_, 0)).DistinctUntilChanged().Subscribe(_ => RotateAngle = _).DisposeWith(Disposable);
            Title = vehicle.Name.Value;
            vehicle.Name.Subscribe(_ => Title = _).DisposeWith(Disposable);
            vehicle.GlobalPosition.Subscribe(_ => UpdateDescription()).DisposeWith(Disposable);
            
        }

        private void UpdateDescription()
        {
            // TODO: add measure item to ILocalization service to print coordinates
            Description = $"Lat:{_vehicle.GlobalPosition.Value.Latitude:F5}\n" +
                          $"Lon:{_vehicle.GlobalPosition.Value.Longitude:F5}\n" +
                          $"Alt:{_vehicle.GlobalPosition.Value.Altitude:F2}\n";
        }

        private void UpdateIcon(VehicleClass type)
        {
            switch (type)
            {
                case VehicleClass.Plane:
                    Icon = MaterialIconKind.Plane;
                    break;
                case VehicleClass.Copter:
                case VehicleClass.Unknown:
                    Icon = MaterialIconKind.Navigation;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }

        }
    }
}