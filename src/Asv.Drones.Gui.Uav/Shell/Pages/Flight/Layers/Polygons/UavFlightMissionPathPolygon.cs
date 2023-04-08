using System.Collections.ObjectModel;
using System.Reactive.Linq;
using Asv.Common;
using Asv.Mavlink;
using Asv.Mavlink.V2.Common;
using DynamicData;
using DynamicData.Binding;
using ReactiveUI;

namespace Asv.Drones.Gui.Uav;

public class UavFlightMissionPathPolygon : FlightAnchorBase
{
    private readonly ReadOnlyObservableCollection<GeoPoint> _items;

    public UavFlightMissionPathPolygon(IVehicle vehicle) : base(vehicle, "flight-mission-polygon")
    {
        vehicle.MissionItems
            .AutoRefreshOnObservable(_ => _.Location)
            .SortBy(_=>_.Index, SortDirection.Ascending, SortOptimisations.ComparesImmutableValuesOnly)
            .Filter(_=>_.Command.Value is MavCmd.MavCmdNavWaypoint or MavCmd.MavCmdNavSplineWaypoint )
            .Transform(_ => _.Location.Value, true)
            .ObserveOn(RxApp.MainThreadScheduler)
            .DisposeMany()
            .Bind(out _items, useReplaceForUpdates:true)
            .Subscribe()
            .DisposeItWith(Disposable);
    }
    
    public override ReadOnlyObservableCollection<GeoPoint> Path => _items;
}