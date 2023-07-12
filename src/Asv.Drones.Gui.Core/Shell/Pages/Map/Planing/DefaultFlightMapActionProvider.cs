using System.ComponentModel.Composition;
using DynamicData;

namespace Asv.Drones.Gui.Core;

[Export(PlaningPageViewModel.UriString,typeof(IViewModelProvider<IMapAction>))]
[PartCreationPolicy(CreationPolicy.NonShared)]
public class DefaultPlaningMapActionProvider:ViewModelProviderBase<IMapAction>
{
    [ImportingConstructor]
    public DefaultPlaningMapActionProvider([ImportMany(PlaningPageViewModel.UriString)]IEnumerable<IMapAction> items)
    {
        Source.AddOrUpdate(items);
    }
}