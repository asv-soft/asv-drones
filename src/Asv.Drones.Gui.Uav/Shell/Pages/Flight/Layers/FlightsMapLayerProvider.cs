using DynamicData;
using System.ComponentModel.Composition;
using System.Reactive.Linq;
using Asv.Drones.Gui.Core;
using DynamicData.PLinq;

namespace Asv.Drones.Gui.Uav
{

    [Export(FlightPageViewModel.UriString, typeof(IViewModelProvider<IMapAnchor>))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class FlightsMapLayerProvider : IViewModelProvider<IMapAnchor>
    {
        [ImportingConstructor]
        public FlightsMapLayerProvider(IMavlinkDevicesService svc,
            ILocalizationService loc,
            [ImportMany]IEnumerable<IFlightUavActionProvider> actions)
        {
            var uav = svc.Vehicles.Transform(_=>new UavAnchor(_,loc,actions)).ChangeKey((k, _) => _.Id).Transform(_=>(IMapAnchor)_);
            var roi = svc.Vehicles.Transform(_ => new RoiAnchor(_)).ChangeKey((k, _) => _.Id).Transform(_ => (IMapAnchor)_);
            var home = svc.Vehicles.Transform(_ => new HomeAnchor(_)).ChangeKey((k, _) => _.Id).Transform(_ => (IMapAnchor)_);
            var goTo = svc.Vehicles.Transform(_ => new GoToAnchor(_)).ChangeKey((k, _) => _.Id).Transform(_ => (IMapAnchor)_);
            var goToLine = svc.Vehicles.Transform(_ => new UavGoToPolygon(_)).ChangeKey((k, _) => _.Id).Transform(_ => (IMapAnchor)_);
            var track = svc.Vehicles.Transform(_ => new UavTrackPolygon(_)).ChangeKey((k, _) => _.Id).Transform(_ => (IMapAnchor)_);
            
            var adsb = svc.AdsbDevices
                .Transform(_ => new AdsbMapLayer(_,loc))
                .DisposeMany()
                .TransformMany(_ => _.Items, _ => _.Id)
                .Transform(_ => (IMapAnchor)_);
                

            var anchors = svc.Vehicles
                .Transform(_ => new UavFlightMissionMapLayer(_))
                .DisposeMany()
                .TransformMany(_ => _.Items, _ => _.Id)
                .Transform(_=>(IMapAnchor)_)
                .DisposeMany();

            var polygons = svc.Vehicles
                .Transform(_ => (IMapAnchor)new UavFlightMissionPathPolygon(_))
                .ChangeKey((k, v) => v.Id)
                .DisposeMany();

            Items = uav.Merge(adsb).Merge(roi).Merge(home).Merge(goTo).Merge(goToLine).Merge(track).Merge(anchors).Merge(polygons);
        }

        public IObservable<IChangeSet<IMapAnchor, Uri>> Items { get; }
    }
}