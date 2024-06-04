using System.Collections.Generic;
using System.Composition;
using Asv.Drones.Gui.Api;
using DynamicData;

namespace Asv.Drones.Gui;

[Export(WellKnownUri.ShellPageMapFlight, typeof(IViewModelProvider<IMapWidget>))]
public class DefaultFlightWidgetProvider : ViewModelProviderBase<IMapWidget>
{
    [ImportingConstructor]
    public DefaultFlightWidgetProvider([ImportMany(WellKnownUri.ShellPageMapFlight)] IEnumerable<IMapWidget> items)
    {
        Source.AddOrUpdate(items);
    }
}