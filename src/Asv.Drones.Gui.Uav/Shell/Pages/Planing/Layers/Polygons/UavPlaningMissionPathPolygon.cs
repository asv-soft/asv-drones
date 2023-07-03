using System.Collections.ObjectModel;
using System.Diagnostics;
using Asv.Common;
using Asv.Mavlink;
using Asv.Mavlink.V2.Common;
using DynamicData;
using DynamicData.Binding;

namespace Asv.Drones.Gui.Uav
{
    public class UavPlaningMissionPathPolygon: PlaningAnchorBase
    {
        private readonly ReadOnlyObservableCollection<GeoPoint> _items;

        public UavPlaningMissionPathPolygon(IVehicleClient vehicle):base(vehicle,"planing-mission-polygon")
        {
            vehicle.Missions.MissionItems
                .AutoRefreshOnObservable(_=> _.Location)
                .SortBy(_=>_.Index, SortDirection.Ascending, SortOptimisations.ComparesImmutableValuesOnly)
                .Filter(_=>_.Command.Value is MavCmd.MavCmdNavWaypoint or MavCmd.MavCmdNavSplineWaypoint )
                .Transform(_ => _.Location.Value, true)
                .DisposeMany()
                .Bind(out _items, useReplaceForUpdates:true)
                .Subscribe()
                .DisposeItWith(Disposable);

            _items.ObserveCollectionChanges().Subscribe(_ =>
            {
                Debug.WriteLine(_.EventArgs.Action.ToString("G"));
            });
        }

        public override ReadOnlyObservableCollection<GeoPoint> Path => _items;
    }
}