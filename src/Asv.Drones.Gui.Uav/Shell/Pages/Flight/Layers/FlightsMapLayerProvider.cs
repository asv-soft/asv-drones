using DynamicData;
using System.ComponentModel.Composition;
using System.Reactive.Linq;
using Asv.Drones.Gui.Core;

namespace Asv.Drones.Gui.Uav
{

    [Export(FlightPageViewModel.UriString, typeof(IViewModelProvider<IMapAnchor>))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class FlightsMapLayerProvider : IViewModelProvider<IMapAnchor>
    {
        
        
        [ImportingConstructor]
        public FlightsMapLayerProvider(IMavlinkDevicesService svc,ILocalizationService loc,[ImportMany]IEnumerable<IFlightUavActionProvider> actions)
        {
            var uav = svc.Vehicles.Transform(_=>new UavAnchor(_,loc,actions)).ChangeKey((k, _) => _.Id).Transform(_=>(IMapAnchor)_);
            var roi = svc.Vehicles.Transform(_ => new RoiAnchor(_)).ChangeKey((k, _) => _.Id).Transform(_ => (IMapAnchor)_);
            var home = svc.Vehicles.Transform(_ => new HomeAnchor(_)).ChangeKey((k, _) => _.Id).Transform(_ => (IMapAnchor)_);
            var goTo = svc.Vehicles.Transform(_ => new GoToAnchor(_)).ChangeKey((k, _) => _.Id).Transform(_ => (IMapAnchor)_);
            var goToLine = svc.Vehicles.Transform(_ => new UavGoToPolygon(_)).ChangeKey((k, _) => _.Id).Transform(_ => (IMapAnchor)_);
            var track = svc.Vehicles.Transform(_ => new UavTrackPolygon(_)).ChangeKey((k, _) => _.Id).Transform(_ => (IMapAnchor)_);
            
            Items = uav.Merge(roi).Merge(home).Merge(goTo).Merge(goToLine).Merge(track);
        }

        public IObservable<IChangeSet<IMapAnchor, Uri>> Items { get; }
    }
}