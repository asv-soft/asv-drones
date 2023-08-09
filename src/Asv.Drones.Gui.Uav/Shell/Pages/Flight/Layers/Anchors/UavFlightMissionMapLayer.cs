using System.Collections.ObjectModel;
using Asv.Common;
using Asv.Drones.Gui.Core;
using Asv.Mavlink;
using DynamicData;

namespace Asv.Drones.Gui.Uav;

public class UavFlightMissionMapLayer : DisposableReactiveObject
{
    private readonly ReadOnlyObservableCollection<UavFlightMissionAnchor> _items;

    public UavFlightMissionMapLayer(IVehicleClient vehicle)
    {
        vehicle.Missions.MissionItems
            .Transform(_ => new UavFlightMissionAnchor(_,vehicle))
            .DisposeMany()
            .Bind(out _items)
            .Subscribe()
            .DisposeItWith(Disposable);
        
        Disposable.AddAction(() =>
        {

        });
    }

    public ReadOnlyObservableCollection<UavFlightMissionAnchor> Items => _items;

}