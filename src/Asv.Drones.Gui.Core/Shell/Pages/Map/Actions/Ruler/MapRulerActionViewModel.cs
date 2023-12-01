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
        //SetUpRuler(false);
    }
        
   static CancellationTokenSource tokenSource = new CancellationTokenSource();
    CancellationToken token = tokenSource.Token;
   
   
    private async void SetUpRuler(bool isVisible)
    {
        
        
        GeoPoint start ;
        GeoPoint stop ;
        if (Map == null) return;
        
        var polygon = Map.Markers.FirstOrDefault(x => x is RulerPolygon) as RulerPolygon;

        if (polygon == null) return;


        if (isVisible == false)
        {
               tokenSource.Cancel();
                polygon.Ruler.Value.Start.OnNext(null);
                polygon.Ruler.Value.Stop.OnNext(null);
                polygon.Ruler.Value.IsVisible.OnNext(false);
                 tokenSource.Dispose();
                return;
        }
        
        if (isVisible)
        {
            tokenSource = new CancellationTokenSource();
            start = await Map.ShowTargetDialog(RS.MapPageViewModel_RulerStartPoint_Description, token);
            if (tokenSource.IsCancellationRequested == true)
            {
                return;
            }
            stop = await Map.ShowTargetDialog(RS.MapPageViewModel_RulerStopPoint_Description, token);
            polygon.Ruler.Value.Start.OnNext(start);
            polygon.Ruler.Value.Stop.OnNext(stop);
        }
        polygon.Ruler.Value.IsVisible.OnNext(isVisible);
    }
    
    [Reactive]
    
    public bool IsRulerEnabled { get; set; }
    
}