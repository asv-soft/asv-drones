using System.ComponentModel.Composition;
using Asv.Drones.Gui.Core;
using DynamicData;

namespace Asv.Drones.Gui.Uav
{
    [Export(PlaningPageViewModel.UriString, typeof(IViewModelProvider<IMapWidget>))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class PlaningMissionWidgetProvider:ViewModelProviderBase<IMapWidget>
    {
        [ImportingConstructor]
        public PlaningMissionWidgetProvider(IMavlinkDevicesService devices,ILogService log)
        {
            devices.Vehicles
                .Transform(_ => (IMapWidget)new PlaningMissionViewModel(_,log))
                .ChangeKey( ((_, v) => v.Id) )
                .PopulateInto(Source);
        }
    }
}