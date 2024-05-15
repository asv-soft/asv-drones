using System.Collections.Generic;
using System.Composition;
using Asv.Drones.Gui.Api;
using DynamicData;

namespace Asv.Drones.Gui;

[Export(WellKnownUri.ShellPageMapFlight, typeof(IViewModelProvider<IMapMenuItem>))]
public class DefaultFlightMenuProvider : ViewModelProviderBase<IMapMenuItem>
{
    [ImportingConstructor]
    public DefaultFlightMenuProvider([ImportMany(WellKnownUri.ShellPageMapFlight)] IEnumerable<IMapMenuItem> items)
    {
        Source.AddOrUpdate(items);
    }
}