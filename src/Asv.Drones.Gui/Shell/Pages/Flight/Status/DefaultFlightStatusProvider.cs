using System.Collections.Generic;
using System.Composition;
using Asv.Drones.Gui.Api;
using DynamicData;

namespace Asv.Drones.Gui;

[Export(WellKnownUri.ShellPageMapFlight, typeof(IViewModelProvider<IMapStatusItem>))]
public class DefaultFlightStatusProvider : ViewModelProviderBase<IMapStatusItem>
{
    [ImportingConstructor]
    public DefaultFlightStatusProvider([ImportMany(WellKnownUri.ShellPageMapFlight)] IEnumerable<IMapStatusItem> items)
    {
        Source.AddOrUpdate(items);
    }
}