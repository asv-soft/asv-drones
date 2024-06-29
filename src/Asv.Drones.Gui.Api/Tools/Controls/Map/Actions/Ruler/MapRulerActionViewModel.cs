using System.Collections.ObjectModel;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Asv.Avalonia.Map;
using Asv.Common;
using Asv.Mavlink.Vehicle;
using Avalonia.Collections;
using Avalonia.Media;
using DynamicData;
using DynamicData.Binding;
using Material.Icons;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui.Api;

public class MapRulerActionViewModel : MapActionBase
{
    private readonly ILocalizationService _loc;
    private CancellationTokenSource? _tokenSource;
    private RulerStartAnchor? _startAnchor;
    private RulerStopAnchor? _stopAnchor;
    private RulerPolygon? _rulerPolygon;

    public MapRulerActionViewModel() : base(WellKnownUri.UndefinedUri)
    {
        DesignTime.ThrowIfNotDesignMode();
    }

    public MapRulerActionViewModel(string id, ILocalizationService loc) : base(id)
    {
        _loc = loc;
        this.WhenValueChanged(m => m.IsRulerEnabled, false)
            .Subscribe(SetUpRuler)
            .DisposeItWith(Disposable);
        Disposable.AddAction(() => { SetUpRuler(false); });
    }

    private async void SetUpRuler(bool isEnabled)
    {
        if (Map == null) return;
        if (isEnabled)
        {
            _tokenSource?.Cancel(false);
            _tokenSource?.Dispose();
            _tokenSource = new CancellationTokenSource();
            try
            {
                var startPoint = await Map.ShowTargetDialog("Select ruler starting point", _tokenSource.Token);
                if (double.IsNaN(startPoint.Altitude))
                {
                    IsRulerEnabled = false;
                    return;
                }

                _startAnchor = new RulerStartAnchor(new Uri(Id, "start"), startPoint);
                Map.AdditionalAnchorsSource.AddOrUpdate(_startAnchor);
                var stopPoint = await Map.ShowTargetDialog("Select ruler stopping point", _tokenSource.Token);
                if (double.IsNaN(stopPoint.Altitude))
                {
                    IsRulerEnabled = false;
                    return;
                }

                _stopAnchor = new RulerStopAnchor(new Uri(Id, "stop"), _startAnchor, _loc, stopPoint);
                Map.AdditionalAnchorsSource.AddOrUpdate(_stopAnchor);

                _rulerPolygon = new RulerPolygon(new Uri(Id, "ruler"), _startAnchor, _stopAnchor);
                Map.AdditionalAnchorsSource.AddOrUpdate(_rulerPolygon);
            }
            catch (TaskCanceledException)
            {
                DisableRulerOnClick();
            }
        }
        else
        {
            DisableRulerOnClick();
        }
    }

    private void DisableRulerOnClick()
    {
        _tokenSource?.Cancel();
        
        if (Map == null) return;
        
        if (_startAnchor != null)
        {
            Map.AdditionalAnchorsSource.Remove(_startAnchor.Id);
            _startAnchor.Dispose();
            _startAnchor = null;
        }

        if (_stopAnchor != null)
        {
            Map.AdditionalAnchorsSource.Remove(_stopAnchor.Id);
            _stopAnchor.Dispose();
            _stopAnchor = null;
        }

        if (_rulerPolygon != null)
        {
            Map.AdditionalAnchorsSource.Remove(_rulerPolygon.Id);
            _rulerPolygon.Dispose();
            _rulerPolygon = null;
        }

        IsRulerEnabled = false;
    }

    [Reactive] public bool IsRulerEnabled { get; set; }
}

public class RulerStartAnchor : MapAnchorBase
{
    public RulerStartAnchor(Uri id, GeoPoint startPoint) : base(id)
    {
        Size = 48;
        BaseSize = 48;
        OffsetX = OffsetXEnum.Center;
        OffsetY = OffsetYEnum.Bottom;
        StrokeThickness = 1;
        IconBrush = Brushes.Indigo;
        Stroke = Brushes.White;
        IsVisible = true;
        Icon = MaterialIconKind.MapMarker;
        IsEditable = true;
        Location = startPoint;
    }
}

public class RulerStopAnchor : MapAnchorBase
{
    public RulerStopAnchor(Uri id, RulerStartAnchor start, ILocalizationService loc, GeoPoint stopPoint) : base(id)
    {
        Size = 48;
        BaseSize = 48;
        OffsetX = OffsetXEnum.Center;
        OffsetY = OffsetYEnum.Bottom;
        StrokeThickness = 1; 
        IconBrush = Brushes.Indigo;
        Stroke = Brushes.White;
        IsVisible = true;
        Icon = MaterialIconKind.MapMarker;
        IsEditable = true;
        Location = stopPoint;
        this.WhenValueChanged(x => x.Location, false)
            .Merge(start.WhenValueChanged(x => x.Location, false))
            .Throttle(TimeSpan.FromMilliseconds(50), RxApp.MainThreadScheduler)
            .Subscribe(x =>
            {
                Title = loc.Distance.FromSiToStringWithUnits(GeoMath.Distance(start.Location, Location));
            }).DisposeItWith(Disposable);
        Title = loc.Distance.FromSiToStringWithUnits(GeoMath.Distance(start.Location, Location));
    }
}

public class RulerPolygon : MapAnchorBase
{
    private readonly ReadOnlyObservableCollection<GeoPoint> _path;

    public RulerPolygon(Uri id, RulerStartAnchor start, RulerStopAnchor stop) : base(id)
    {
        ZOrder = -1000;
        OffsetX = 0;
        OffsetY = 0;
        PathOpacity = 0.6;
        StrokeThickness = 5;
        Stroke = Brushes.Purple;
        IsVisible = true;
        StrokeDashArray = new AvaloniaList<double>(2, 2);

        var cache = new SourceList<GeoPoint>().DisposeItWith(Disposable);
        cache.Add(new GeoPoint(0, 0, 0));
        cache.Add(new GeoPoint(0, 0, 0));

        start.WhenValueChanged(x => x.Location).Subscribe(x => cache.ReplaceAt(0, x)).DisposeItWith(Disposable);
        stop.WhenValueChanged(x => x.Location).Subscribe(x => cache.ReplaceAt(1, x)).DisposeItWith(Disposable);

        cache.Connect()
            .Bind(out _path)
            .Subscribe()
            .DisposeWith(Disposable);
    }

    public override ReadOnlyObservableCollection<GeoPoint> Path => _path;
}