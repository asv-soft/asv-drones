using System.Collections.Generic;
using System.Composition;
using Asv.Drones.Gui.Api;
using DynamicData;

namespace Asv.Drones.Gui;

[Export(WellKnownUri.ShellPageMapPlaning, typeof(IViewModelProvider<IMapWidget>))]
public class DefaultPlaningWidgetProvider : ViewModelProviderBase<IMapWidget>
{
    [ImportingConstructor]
    public DefaultPlaningWidgetProvider([ImportMany(WellKnownUri.ShellPageMapPlaning)] IEnumerable<IMapWidget> items)
    {
        Source.AddOrUpdate(items);
    }
}