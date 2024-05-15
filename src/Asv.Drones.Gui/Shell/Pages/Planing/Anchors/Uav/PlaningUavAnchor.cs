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
    public class PlaningUavAnchor : MapAnchorBase
    {
        private readonly IVehicleClient _vehicle;
        private readonly ILocalizationService _loc;

        public PlaningUavAnchor(IVehicleClient vehicle, ILocalizationService loc)
            : base($"{WellKnownUri.ShellPageMapPlaning}/{vehicle.FullId}")
        {
            _vehicle = vehicle;
            _loc = loc;
            Size = 48;
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
                              _loc.Latitude.FromSiToStringWithUnits(_vehicle.Position.Current.Value.Latitude)) + "\n" +
                          string.Format(RS.UavAnchor_Longitude,
                              _loc.Longitude.FromSiToStringWithUnits(_vehicle.Position.Current.Value.Longitude)) +
                          "\n" +
                          string.Format(RS.UavAnchor_GNSS_Altitude,
                              _loc.Altitude.FromSiToStringWithUnits(_vehicle.Position.Current.Value.Altitude)) + "\n" +
                          string.Format(RS.UavAnchor_AGL_Altitude,
                              _loc.Altitude.FromSiToStringWithUnits(_vehicle.Position.AltitudeAboveHome.Value));
        }
    }
}