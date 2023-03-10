using System.Collections.ObjectModel;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Asv.Common;
using Asv.Drones.Gui.Core;
using Asv.Mavlink;
using Avalonia.Media;
using DynamicData;
using ReactiveUI;

namespace Asv.Drones.Gui.Uav
{
    public class UavTrackPolygon : FlightAnchorBase
    {
        private readonly ReadOnlyObservableCollection<GeoPoint> _path;

        public UavTrackPolygon(IVehicle vehicle) : base(vehicle, "track-polygon")
        {
            ZOrder = -1000;
            OffsetX = 0;
            OffsetY = 0;
            PathOpacity = 0.6;
            Stroke = Brushes.Honeydew;
            IsVisible = true;
            vehicle.GlobalPosition
                .Where(_ => _.Latitude != 0 && _.Longitude != 0)
                .Sample(TimeSpan.FromSeconds(1), RxApp.MainThreadScheduler)
                .Select(_ => _)
                .ToObservableChangeSet(limitSizeTo:100) // TODO: move history size to settings
                .Bind(out _path)
                .Subscribe()
                .DisposeWith(Disposable);
        }

        public override ReadOnlyObservableCollection<GeoPoint> Path => _path;
    }
}