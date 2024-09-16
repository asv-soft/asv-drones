using System;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using Asv.Common;
using Asv.Mavlink;
using Avalonia.Collections;
using Avalonia.Media;
using DynamicData;

namespace Asv.Drones.Gui
{
    public class UavGoToPolygon : FlightAnchorBase
    {
        private readonly ReadOnlyObservableCollection<GeoPoint> _path;

        public UavGoToPolygon(IVehicleClient vehicle) : base(vehicle, "goto-polygon")
        {
            ZOrder = -1000;
            OffsetX = 0;
            OffsetY = 0;
            PathOpacity = 0.6;
            StrokeThickness = 1;
            Stroke = Brushes.Honeydew;
            IsVisible = false;
            StrokeDashArray = new AvaloniaList<double>(2, 2);

            var cache = new SourceList<GeoPoint>().DisposeItWith(Disposable);
            cache.Add(new GeoPoint(0, 0, 0));
            cache.Add(new GeoPoint(0, 0, 0));

            vehicle.Position.Target.Select(_ => _.HasValue).Subscribe(_ => IsVisible = _).DisposeItWith(Disposable);
            // ReSharper disable once PossibleInvalidOperationException
            vehicle.Position.Target.Where(_ => _.HasValue).Subscribe(_ => cache.ReplaceAt(1, _.Value))
                .DisposeItWith(Disposable);
            vehicle.Position.Current.Where(_ => vehicle.Position.Target.Value.HasValue)
                .Subscribe(_ => cache.ReplaceAt(0, _)).DisposeItWith(Disposable);

            cache.Connect()
                .Bind(out _path)
                .Subscribe()
                .DisposeItWith(Disposable);
        }

        public override ReadOnlyObservableCollection<GeoPoint> Path => _path;
    }
}