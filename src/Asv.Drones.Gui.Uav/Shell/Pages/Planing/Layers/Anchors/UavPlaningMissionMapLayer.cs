using System.Collections.ObjectModel;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Asv.Common;
using Asv.Drones.Gui.Core;
using Asv.Mavlink;
using DynamicData;
using ReactiveUI;

namespace Asv.Drones.Gui.Uav
{
    public class UavPlaningMissionMapLayer : DisposableViewModelBase
    {
        private readonly ReadOnlyObservableCollection<UavPlaningMissionAnchor> _items;

        public UavPlaningMissionMapLayer(IVehicleClient vehicle)
        {
            vehicle.Missions.MissionItems
                .Transform(_=>new UavPlaningMissionAnchor(_,vehicle))
                .ObserveOn(RxApp.MainThreadScheduler)
                .DisposeMany()
                .Bind(out _items)
                .Subscribe()
                .DisposeItWith(Disposable);
            Disposable.AddAction(() =>
            {

            });
        }

        public ReadOnlyObservableCollection<UavPlaningMissionAnchor> Items => _items;

    }
}