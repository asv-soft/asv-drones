using System.ComponentModel.Composition;
using Asv.Common;
using Avalonia.Controls.Shapes;
using DocumentFormat.OpenXml.Spreadsheet;
using DynamicData.Binding;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui.Core;

[Export(FlightPageViewModel.UriString,typeof(IMapAction))]
[Export(PlaningPageViewModel.UriString,typeof(IMapAction))]
[PartCreationPolicy(CreationPolicy.NonShared)]
public class MapRulerActionViewModel:MapActionBase
{
    
    [ImportingConstructor]
    public MapRulerActionViewModel( ILogService log) : base("asv:shell.page.map.action.ruler")
    {
       
        this.WhenValueChanged(_ => _.IsRulerEnabled)
            .Subscribe(SetUpRuler)
            .DisposeItWith(Disposable);
    }

    protected override void InternalWhenMapLoaded(IMap context)
    {
       // base.InternalWhenMapLoaded(context);
      //  SetUpRuler(false);
    }
    
    private async void SetUpRuler(bool isEnabled)
    {
        if (Map == null) return;
        
        var polygon = Map.Markers.FirstOrDefault(x => x is RulerPolygon) as RulerPolygon;

        if (polygon == null) return;
        
        if (isEnabled == false)
        {
            _tokenSource.Cancel();
            polygon.Ruler.Value.Start.OnNext(null);
            polygon.Ruler.Value.Stop.OnNext(null);
            _tokenSource.Dispose();
        }
        
        if(isEnabled)
        {
                _tokenSource = new CancellationTokenSource();
                _token = _tokenSource.Token;
                var start = await Map.ShowTargetDialog(RS.MapPageViewModel_RulerStartPoint_Description, _token);
                if (_tokenSource.IsCancellationRequested) return;
                var stop = await Map.ShowTargetDialog(RS.MapPageViewModel_RulerStopPoint_Description, _token);
                if (_tokenSource.IsCancellationRequested) return;
                polygon.Ruler.Value.Start.OnNext(start);
                polygon.Ruler.Value.Stop.OnNext(stop);
        }
        polygon.Ruler.Value.IsVisible.OnNext(isEnabled);
    }

    private static CancellationTokenSource _tokenSource;
    private CancellationToken _token;

    [Reactive] 
   
    public bool IsRulerEnabled { get; set; }
    
}