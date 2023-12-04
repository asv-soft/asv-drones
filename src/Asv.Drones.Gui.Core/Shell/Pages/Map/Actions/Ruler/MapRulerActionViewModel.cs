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
    private async void SetUpRuler(bool isEnabled)
    {
        if (Map == null) return;
        var polygon = Map.Markers.FirstOrDefault(x => x is RulerPolygon) as RulerPolygon;
        if (polygon == null) return;
        
        _tokenSource.Cancel();
        _tokenSource = new CancellationTokenSource();
        if(isEnabled)
        {
            try
            {
                var start = await Map.ShowTargetDialog(RS.MapPageViewModel_RulerStartPoint_Description,
                    _tokenSource.Token);
                var stop = await Map.ShowTargetDialog(RS.MapPageViewModel_RulerStopPoint_Description,
                    _tokenSource.Token);
                polygon.Ruler.Value.Start.OnNext(start);
                polygon.Ruler.Value.Stop.OnNext(stop);
            }
            catch (TaskCanceledException)
            {
                return;
            }
        }
        polygon.Ruler.Value.IsVisible.OnNext(isEnabled);
    }
    private static CancellationTokenSource _tokenSource = new CancellationTokenSource();
    [Reactive] 
    public bool IsRulerEnabled { get; set; }
}
