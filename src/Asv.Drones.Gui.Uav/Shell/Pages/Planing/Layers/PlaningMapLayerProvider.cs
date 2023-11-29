using System.ComponentModel.Composition;
using System.Reactive.Linq;
using Asv.Drones.Gui.Core;
using DynamicData;

namespace Asv.Drones.Gui.Uav
{
    [Export(PlaningPageViewModel.UriString,typeof(IViewModelProvider<IMapAnchor>))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class PlaningMapLayerProvider : IViewModelProvider<IMapAnchor>
    {
        [ImportingConstructor]
        public PlaningMapLayerProvider(IMavlinkDevicesService svc, ILocalizationService loc, [ImportMany]IEnumerable<IUavActionProvider> actions)
        {
            var uav = svc.Vehicles.Transform(_=>new FlightUavAnchor(_,loc,actions)).ChangeKey((k, _) => _.Id).Transform(_=>(IMapAnchor)_);
            var home = svc.Vehicles.Transform(_ => new HomeAnchor(_)).ChangeKey((k, _) => _.Id).Transform(_ => (IMapAnchor)_);
            var goTo = svc.Vehicles.Transform(_ => new GoToAnchor(_)).ChangeKey((k, _) => _.Id).Transform(_ => (IMapAnchor)_);
            var goToLine = svc.Vehicles.Transform(_ => new UavGoToPolygon(_)).ChangeKey((k, _) => _.Id).Transform(_ => (IMapAnchor)_);
            var track = svc.Vehicles.Transform(_ => new UavTrackPolygon(_)).ChangeKey((k, _) => _.Id).Transform(_ => (IMapAnchor)_);

            var adsb = svc.AdsbDevices
                .Transform(_ => new AdsbMapLayer(_,loc))
                .DisposeMany()
                .TransformMany(_ => _.Items, _ => _.Id)
                .Transform(_ => (IMapAnchor)_);
            
            Items = uav.Merge(home).Merge(goTo).Merge(goToLine).Merge(track).Merge(adsb);
        }

        public IObservable<IChangeSet<IMapAnchor, Uri>> Items { get; }

        public void Dispose()
        {
            
        }
    }
}