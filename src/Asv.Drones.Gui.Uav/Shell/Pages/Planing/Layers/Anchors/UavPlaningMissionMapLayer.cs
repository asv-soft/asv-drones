using System.Collections.ObjectModel;
using Asv.Common;
using Asv.Drones.Gui.Core;
using Asv.Mavlink;
using DynamicData;

namespace Asv.Drones.Gui.Uav
{
    public class UavPlaningMissionMapLayer : DisposableReactiveObject
    {
        private readonly ReadOnlyObservableCollection<UavPlaningMissionAnchor> _items;

        public UavPlaningMissionMapLayer(IVehicleClient vehicle)
        {
            vehicle.Missions.MissionItems
                .Transform(_=>new UavPlaningMissionAnchor(_,vehicle))
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