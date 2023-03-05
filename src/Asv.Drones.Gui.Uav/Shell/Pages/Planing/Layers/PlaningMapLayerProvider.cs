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
        public PlaningMapLayerProvider(IMavlinkDevicesService svc)
        {
            var anchors = svc.Vehicles
                .Transform(_ => new UavMissionMapLayer(_))
                .DisposeMany()
                .TransformMany(_ => _.Items, _ => _.Id)
                .Transform(_=>(IMapAnchor)_);
            var polygon = svc
                .Vehicles
                .Transform(_ => (IMapAnchor)new UavMissionPathPolygon(_))
                .ChangeKey((k, v) => v.Id)
                .DisposeMany();
            Items = anchors.Merge(polygon);

        }

        public IObservable<IChangeSet<IMapAnchor, Uri>> Items { get; }
    }
}