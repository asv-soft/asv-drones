using System.ComponentModel.Composition;
using Asv.Cfg;
using Asv.Drones.Gui.Core;
using Asv.Drones.Gui.Uav;
using DynamicData;

namespace Asv.Drones.Gui.Sdr;



[Export(FlightPageViewModel.UriString, typeof(IViewModelProvider<IMapWidget>))]
[PartCreationPolicy(CreationPolicy.NonShared)]
public class CustomFlightMissionWidgetProvider:ViewModelProviderBase<IMapWidget>
{
    [ImportingConstructor]
    public CustomFlightMissionWidgetProvider(
        IMavlinkDevicesService devices,ILogService log,
        ILocalizationService localization,
        IConfiguration configuration,
        [ImportMany]IEnumerable<ISdrRttProvider> rtt)
    {
        devices.Payloads
            .Transform(_ => (IMapWidget)new CustomFlightSdrViewModel(_,log, localization,configuration,rtt))
            .ChangeKey( ((_, v) => v.Id) )
            .PopulateInto(Source);
    }
}