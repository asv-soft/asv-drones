using System.Collections.ObjectModel;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Asv.Common;
using Asv.Drones.Gui.Core;
using Asv.Mavlink;
using Avalonia.Collections;
using Avalonia.Media;
using DynamicData;

namespace Asv.Drones.Gui.Uav
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
            StrokeDashArray = new AvaloniaList<double>(2,2);

            var cache = new SourceList<GeoPoint>().DisposeItWith(Disposable);
            cache.Add(new GeoPoint(0, 0, 0));
            cache.Add(new GeoPoint(0, 0, 0));

            vehicle.Position.Target.Select(_ => _.HasValue).Subscribe(_ => IsVisible = _).DisposeWith(Disposable);
            vehicle.Position.Target.Where(_ => _.HasValue).Subscribe(_ => cache.ReplaceAt(1,_.Value)).DisposeWith(Disposable);
            vehicle.Position.Current.Where(_=>vehicle.Position.Target.Value.HasValue).Subscribe(_=>cache.ReplaceAt(0, _)).DisposeWith(Disposable);

            cache.Connect()
                .Bind(out _path)
                .Subscribe()
                .DisposeWith(Disposable);

        }

        public override ReadOnlyObservableCollection<GeoPoint> Path => _path;
    }
}