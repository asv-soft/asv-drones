using System.Collections.Generic;
using System.Composition;
using Asv.Drones.Gui.Api;
using DynamicData;

namespace Asv.Drones.Gui;

[Export(WellKnownUri.ShellPageMapFlight, typeof(IViewModelProvider<IMapAction>))]
public class DefaultFlightActionsProvider : ViewModelProviderBase<IMapAction>
{
    [ImportingConstructor]
    public DefaultFlightActionsProvider([ImportMany(WellKnownUri.ShellPageMapFlight)] IEnumerable<IMapAction> items)
    {
        Source.AddOrUpdate(items);
    }
}