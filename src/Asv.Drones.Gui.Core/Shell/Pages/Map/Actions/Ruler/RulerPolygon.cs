using System.Collections.ObjectModel;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Asv.Common;
using Avalonia.Collections;
using Avalonia.Media;
using DynamicData;

namespace Asv.Drones.Gui.Core;

public class RulerPolygon : MapAnchorBase
{
    private readonly ReadOnlyObservableCollection<GeoPoint> _path;
    
    public RulerPolygon(Ruler ruler) : base(new Uri(FlightPageViewModel.UriString + "/layer/ruler-polygon"))
    {
        Ruler = new RxValue<Ruler>().DisposeItWith(Disposable);
        
        ZOrder = -1000;
        OffsetX = 0;
        OffsetY = 0;
        PathOpacity = 0.6;
        StrokeThickness = 5;
        Stroke = Brushes.Purple;
        IsVisible = false;
        StrokeDashArray = new AvaloniaList<double>(2,2);

        var cache = new SourceList<GeoPoint>().DisposeItWith(Disposable);
        cache.Add(new GeoPoint(0, 0, 0));
        cache.Add(new GeoPoint(0, 0, 0));

        ruler.IsVisible.Where(_ => _.HasValue).Subscribe(_ => IsVisible = _.Value).DisposeItWith(Disposable);
        ruler.Start.Where(_ => _.HasValue).Subscribe(_ => cache.ReplaceAt(0, _.Value)).DisposeItWith(Disposable);
        ruler.Stop.Where(_ => _.HasValue).Subscribe(_ => cache.ReplaceAt(1, _.Value)).DisposeItWith(Disposable);

        cache.Connect()
            .Bind(out _path)
            .Subscribe()
            .DisposeWith(Disposable);
        
        Ruler.OnNext(ruler);
    }

    public override ReadOnlyObservableCollection<GeoPoint> Path => _path;
    
    public IRxEditableValue<Ruler> Ruler { get; }
}