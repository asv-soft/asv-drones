using System.Collections.ObjectModel;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Asv.Avalonia.Map;
using Asv.Common;
using Asv.Drones.Gui.Core;
using Asv.Mavlink;
using Avalonia.Media;
using DynamicData;
using Material.Icons;

namespace Asv.Drones.Gui.Uav
{
    public class UavAnchor : FlightAnchorBase
    {
        private ReadOnlyObservableCollection<MapAnchorActionViewModel> _actions;
        private readonly IEnumerable<IFlightUavActionProvider> _actionsProviders;
        private readonly ILocalizationService _loc;

        public const string BaseUriString = "asv:shell.pages.layers.anchors.uavanchor";
        public static readonly Uri BaseUri = new(BaseUriString);

        public UavAnchor(IVehicle vehicle,ILocalizationService loc,IEnumerable<IFlightUavActionProvider> actionsProviders)
            :base(vehicle,"uav")
        {
            _actionsProviders = actionsProviders;
            _loc = loc;
            Size = 48;
            OffsetX = OffsetXEnum.Center;
            OffsetY = OffsetYEnum.Center;
            IconBrush = Brushes.Red;
            IsVisible = true;
            vehicle.Class.Subscribe(_ => Icon = MavlinkHelper.GetIcon(_)).DisposeItWith(Disposable);
            vehicle.GlobalPosition.Subscribe(_ => Location = _).DisposeWith(Disposable);
            vehicle.Yaw.Select(_ => Math.Round(_, 0)).DistinctUntilChanged().Subscribe(_ => RotateAngle = _).DisposeWith(Disposable);
            Title = vehicle.Name.Value;
            vehicle.Name.Subscribe(_ => Title = _).DisposeWith(Disposable);
            vehicle.GlobalPosition.Subscribe(_ => UpdateDescription()).DisposeWith(Disposable);
            
        }
        
        private void UpdateDescription()
        {
            
            // TODO: Localize
            Description = $"Lat: {_loc.LatitudeAndLongitude.FromSIToStringWithUnits(Vehicle.GlobalPosition.Value.Latitude)}\n" +
                          $"Lon: {_loc.LatitudeAndLongitude.FromSIToStringWithUnits(Vehicle.GlobalPosition.Value.Longitude)}\n" +
                          $"Alt: {_loc.Altitude.FromSIToStringWithUnits(Vehicle.GlobalPosition.Value.Altitude)}\n";
        }

        protected override void InternalWhenMapLoaded(IMap map)
        {
            _actionsProviders
                .SelectMany(_ => _.CreateActions(Vehicle, map))
                .Cast<MapAnchorActionViewModel>()
                .OrderBy(_=>_.Order)
                .AsObservableChangeSet()
                .Bind(out _actions)
                .DisposeMany()
                .Subscribe()
                .DisposeWith(Disposable);
        }
        
        public override ReadOnlyObservableCollection<MapAnchorActionViewModel> Actions => _actions;
        
        
    }
}