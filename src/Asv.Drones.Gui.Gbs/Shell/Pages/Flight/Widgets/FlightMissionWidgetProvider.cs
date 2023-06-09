using System.ComponentModel.Composition;
using System.Reactive.Subjects;
using Asv.Cfg;
using Asv.Common;
using Asv.Drones.Gui.Core;
using Asv.Drones.Gui.Uav;
using DynamicData;
using DynamicData.Binding;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui.Gbs
{
    [Export(FlightPageViewModel.UriString, typeof(IViewModelProvider<IMapWidget>))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class FlightMissionWidgetProvider:ViewModelProviderBase<IMapWidget>
    {
        [ImportingConstructor]
        public FlightMissionWidgetProvider(
            IMavlinkDevicesService devices,ILogService log,
            ILocalizationService localization,
            IConfiguration configuration,
            [ImportMany]IEnumerable<IGbsRttItemProvider> rttItems)
        {
            Collapsed = new RxValue<bool>(false);
            
            devices.BaseStations
                .Transform(_ => 
                    (IMapWidget)new FlightGbsViewModel(_, log, localization, configuration, Collapsed, rttItems))
                .ChangeKey((_, v) => v.Id)
                .PopulateInto(Source)
                .DisposeItWith(Disposable);
            
            devices.BaseStations
                .Transform(_ => 
                    (IMapWidget)new FlightGbsMinimizedViewModel(_, log, localization, Collapsed, rttItems))
                .ChangeKey((_, v) => v.Id)
                .PopulateInto(Source)
                .DisposeItWith(Disposable);
        }
        
        [Reactive]
        public RxValue<bool> Collapsed { get; set; }
    }
}