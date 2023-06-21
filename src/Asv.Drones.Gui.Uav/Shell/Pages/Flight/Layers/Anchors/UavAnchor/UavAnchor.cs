using System.Collections.ObjectModel;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Asv.Avalonia.Map;
using Asv.Drones.Gui.Core;
using Asv.Mavlink;
using Avalonia.Media;
using DynamicData;
using MavlinkHelper = Asv.Drones.Gui.Core.MavlinkHelper;

namespace Asv.Drones.Gui.Uav
{
    public class UavAnchor : FlightAnchorBase
    {
        private ReadOnlyObservableCollection<MapAnchorActionViewModel> _actions;
        private readonly IEnumerable<IFlightUavActionProvider> _actionsProviders;
        private readonly ILocalizationService _loc;

        public const string BaseUriString = "asv:shell.pages.layers.anchors.uavanchor";
        public static readonly Uri BaseUri = new(BaseUriString);

        public UavAnchor(IVehicleClient vehicle,ILocalizationService loc,IEnumerable<IFlightUavActionProvider> actionsProviders)
            :base(vehicle,"uav")
        {
            _actionsProviders = actionsProviders;
            _loc = loc;
            Size = 48;
            OffsetX = OffsetXEnum.Center;
            OffsetY = OffsetYEnum.Center;
            StrokeThickness = 1;
            Stroke = Brushes.Honeydew;
            IconBrush = Brushes.Red;
            IsVisible = true;
            Icon = MavlinkHelper.GetIcon(vehicle.Class);
            vehicle.Position.Current.Subscribe(_ => Location = _).DisposeWith(Disposable);
            vehicle.Position.Yaw.Select(_ => Math.Round(_, 0)).DistinctUntilChanged().Subscribe(_ => RotateAngle = _).DisposeWith(Disposable);
            Title = vehicle.Name.Value;
            vehicle.Name.Subscribe(_ => Title = _).DisposeWith(Disposable);
            vehicle.Position.Current.Subscribe(_ => UpdateDescription()).DisposeWith(Disposable);
        }
        
        
        
        private void UpdateDescription()
        {
            Description = string.Format(RS.UavAnchor_Latitude, _loc.Latitude.FromSiToStringWithUnits(Vehicle.Position.Current.Value.Latitude)) + "\n" +
                          string.Format(RS.UavAnchor_Longitude, _loc.Longitude.FromSiToStringWithUnits(Vehicle.Position.Current.Value.Longitude)) + "\n" +
                          string.Format(RS.UavAnchor_GNSS_Altitude, _loc.Altitude.FromSiToStringWithUnits(Vehicle.Position.Current.Value.Altitude)) + "\n" +
                          string.Format(RS.UavAnchor_AGL_Altitude, _loc.Altitude.FromSiToStringWithUnits(Vehicle.Position.AltitudeAboveHome.Value));

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