using System.ComponentModel.Composition;
using DynamicData;

namespace Asv.Drones.Gui.Core;

[Export(FlightPageViewModel.UriString, typeof(IViewModelProvider<IMapAnchor>))]
[PartCreationPolicy(CreationPolicy.NonShared)]
public class RulerFlightMapLayerProvider : ViewModelProviderBase<IMapAnchor>
{
    [ImportingConstructor]
    public RulerFlightMapLayerProvider(ILocalizationService loc)
    {
        var ruler = new Ruler();
        
        Source.AddOrUpdate(new RulerAnchor("1", ruler, RulerPosition.Start, loc));
        Source.AddOrUpdate(new RulerAnchor("2", ruler, RulerPosition.Stop, loc));
        Source.AddOrUpdate(new RulerPolygon(ruler));
    }
}