using System.Collections.Generic;
using System.Composition;
using Asv.Drones.Gui.Api;
using DynamicData;

namespace Asv.Drones.Gui;

[Export(WellKnownUri.ShellPageMapPlaning, typeof(IViewModelProvider<IMapAction>))]
public class DefaultPlaningMapActionProvider : ViewModelProviderBase<IMapAction>
{
    [ImportingConstructor]
    public DefaultPlaningMapActionProvider([ImportMany(WellKnownUri.ShellPageMapPlaning)] IEnumerable<IMapAction> items)
    {
        Source.AddOrUpdate(items);
    }
}