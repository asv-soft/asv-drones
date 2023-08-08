using System.ComponentModel.Composition;
using DynamicData;

namespace Asv.Drones.Gui.Core;

[Export(FlightPageViewModel.UriString,typeof(IViewModelProvider<IMapAction>))]
[PartCreationPolicy(CreationPolicy.NonShared)]
public class DefaultFlightMapActionProvider:ViewModelProviderBase<IMapAction>
{
    [ImportingConstructor]
    public DefaultFlightMapActionProvider([ImportMany(FlightPageViewModel.UriString)]IEnumerable<IMapAction> items)
    {
        Source.AddOrUpdate(items);
    }
}