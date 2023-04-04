using System.Collections.ObjectModel;
using System.Reactive.Linq;
using Asv.Common;
using Asv.Drones.Gui.Core;
using Asv.Mavlink;
using DynamicData;
using ReactiveUI;

namespace Asv.Drones.Gui.Uav;

public class UavFlightMissionMapLayer : DisposableViewModelBase
{
    private readonly ReadOnlyObservableCollection<UavFlightMissionAnchor> _items;

    public UavFlightMissionMapLayer(IVehicle vehicle)
    {
        vehicle.MissionItems
            .Transform(_ => new UavFlightMissionAnchor(_,vehicle))
            .ObserveOn(RxApp.MainThreadScheduler)
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