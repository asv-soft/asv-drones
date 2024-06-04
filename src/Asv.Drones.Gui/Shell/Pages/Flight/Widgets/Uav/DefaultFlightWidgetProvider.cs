using System.Collections.Generic;
using System.Composition;
using Asv.Cfg;
using Asv.Common;
using Asv.Drones.Gui.Api;
using DynamicData;

namespace Asv.Drones.Gui;

[Export(WellKnownUri.ShellPageMapFlight, typeof(IViewModelProvider<IMapWidget>))]
public class FlightUavWidgetProvider : ViewModelProviderBase<IMapWidget>
{
    [ImportingConstructor]
    public FlightUavWidgetProvider(IMavlinkDevicesService devices, ILogService log, ILocalizationService loc, IConfiguration cfg,
        [ImportMany] IEnumerable<IUavRttItemProvider> rttItems)
    {
        devices.Vehicles
            .Transform(_ => (IMapWidget)new FlightUavViewModel(_, log, loc, cfg, rttItems))
            .ChangeKey(((_, v) => v.Id))
            .PopulateInto(Source)
            .DisposeItWith(Disposable);
    }
}