using System.ComponentModel.Composition;
using Asv.Cfg;
using Asv.Drones.Gui.Core;
using Asv.Drones.Gui.Uav;
using DynamicData;

namespace Asv.Drones.Gui.Sdr
{
    // [Export(FlightPageViewModel.UriString, typeof(IViewModelProvider<IMapWidget>))]
    // [PartCreationPolicy(CreationPolicy.NonShared)]
    public class FlightMissionWidgetProvider:ViewModelProviderBase<IMapWidget>
    {
        // [ImportingConstructor]
        public FlightMissionWidgetProvider(
            IMavlinkDevicesService devices,ILogService log,
            ILocalizationService localization,
            IConfiguration configuration,
            [ImportMany]IEnumerable<ISdrRttItemProvider> rttItems)
        {
            devices.Payloads
                .Transform(_ => (IMapWidget)new FlightSdrViewModel(_,log, localization,configuration,rttItems))
                .ChangeKey( ((_, v) => v.Id) )
                .PopulateInto(Source);
        }
    }
}