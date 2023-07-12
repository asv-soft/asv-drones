using System.ComponentModel.Composition;
using Asv.Common;
using DynamicData.Binding;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui.Core;

[Export(FlightPageViewModel.UriString,typeof(IMapAction))]
[Export(PlaningPageViewModel.UriString,typeof(IMapAction))]
[PartCreationPolicy(CreationPolicy.NonShared)]
public class MapRulerActionViewModel:MapActionBase
{
  
    [ImportingConstructor]
    public MapRulerActionViewModel() : base("asv:shell.page.map.action.ruler")
    {
        this.WhenValueChanged(_ => _.IsRulerEnabled)
            .Subscribe(SetUpRuler)
            .DisposeItWith(Disposable);
    }

    protected override void InternalWhenMapLoaded(IMap context)
    {
        base.InternalWhenMapLoaded(context);
        SetUpRuler(true);
    }

    private async void SetUpRuler(bool isVisible)
    {
        if (Map == null) return;
        
        var polygon = Map.Markers.FirstOrDefault(x => x is RulerPolygon) as RulerPolygon;
        if (polygon == null) return;
           
        if (isVisible)
        {
            var start = await Map.ShowTargetDialog(RS.MapPageViewModel_RulerStartPoint_Description, CancellationToken.None);
            var stop = await Map.ShowTargetDialog(RS.MapPageViewModel_RulerStopPoint_Description, CancellationToken.None);

            polygon.Ruler.Value.Start.OnNext(start);
            polygon.Ruler.Value.Stop.OnNext(stop);
        }
            
        polygon.Ruler.Value.IsVisible.OnNext(isVisible);
    }
    
    [Reactive]
    public bool IsRulerEnabled { get; set; }
    
}