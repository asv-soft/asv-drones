using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using Asv.Avalonia.Map;
using Asv.Common;
using Asv.Drones.Gui.Api;
using Asv.Mavlink;
using Avalonia.Media;
using DynamicData;
using MavlinkHelper = Asv.Drones.Gui.Api.MavlinkHelper;

namespace Asv.Drones.Gui
{
    public class FlightUavAnchor : FlightAnchorBase
    {
        private ReadOnlyObservableCollection<MapAnchorActionViewModel>? _actions;
        private readonly IEnumerable<IUavActionProvider> _actionsProviders;
        private readonly ILocalizationService _loc;

        public FlightUavAnchor(IVehicleClient vehicle, ILocalizationService loc,
            IEnumerable<IUavActionProvider> actionsProviders)
            : base(vehicle, "uav")
        {
            _actionsProviders = actionsProviders;
            _loc = loc;
            Size = 48;
            BaseSize = 48;
            OffsetX = OffsetXEnum.Center;
            OffsetY = OffsetYEnum.Center;
            StrokeThickness = 1;
            Stroke = Brushes.Honeydew;
            IconBrush = Brushes.Red;
            IsVisible = true;
            Icon = MavlinkHelper.GetIcon(vehicle.Class);
            vehicle.Position.Current.Subscribe(_ => Location = _).DisposeItWith(Disposable);
            vehicle.Position.Yaw.Select(_ => Math.Round(_, 0)).DistinctUntilChanged().Subscribe(v =>
            {
                if (vehicle.Class == DeviceClass.Plane)
                {
                    RotateAngle = v - 45;
                }
                else
                {
                    RotateAngle = v;
                }
            }).DisposeItWith(Disposable);
            Title = vehicle.Name.Value;
            vehicle.Name.Subscribe(_ => Title = _).DisposeItWith(Disposable);
            vehicle.Position.Current.Subscribe(_ => UpdateDescription()).DisposeItWith(Disposable);
        }


        private void UpdateDescription()
        {
            Description = string.Format(RS.UavAnchor_Latitude,
                              _loc.Latitude.FromSiToStringWithUnits(Vehicle.Position.Current.Value.Latitude)) + "\n" +
                          string.Format(RS.UavAnchor_Longitude,
                              _loc.Longitude.FromSiToStringWithUnits(Vehicle.Position.Current.Value.Longitude)) + "\n" +
                          string.Format(RS.UavAnchor_GNSS_Altitude,
                              _loc.Altitude.FromSiToStringWithUnits(Vehicle.Position.Current.Value.Altitude)) + "\n" +
                          string.Format(RS.UavAnchor_AGL_Altitude,
                              _loc.Altitude.FromSiToStringWithUnits(Vehicle.Position.AltitudeAboveHome.Value));
        }

        protected override void InternalWhenMapLoaded(IMap map)
        {
            _actionsProviders
                .SelectMany(_ => _.CreateActions(Vehicle, map))
                .Cast<MapAnchorActionViewModel>()
                .OrderBy(_ => _.Order)
                .AsObservableChangeSet()
                .Bind(out _actions)
                .DisposeMany()
                .Subscribe()
                .DisposeItWith(Disposable);
        }

        public override ReadOnlyObservableCollection<MapAnchorActionViewModel> Actions => _actions;
    }
}