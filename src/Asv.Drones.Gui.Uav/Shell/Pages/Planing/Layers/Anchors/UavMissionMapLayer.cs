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
    public class UavMissionMapLayer:DisposableViewModelBase
    {
        private readonly ReadOnlyObservableCollection<UavMissionAnchor> _items;

        public UavMissionMapLayer(IVehicle vehicle)
        {
            vehicle.MissionItems
                .Transform(_=>new UavMissionAnchor(_,vehicle))
                .ObserveOn(RxApp.MainThreadScheduler)
                .DisposeMany()
                .Bind(out _items)
                .Subscribe()
                .DisposeItWith(Disposable);
            Disposable.AddAction(() =>
            {

            });
        }

        public ReadOnlyObservableCollection<UavMissionAnchor> Items => _items;

    }
}