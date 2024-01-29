using Asv.Common;
using Asv.Mavlink;
using ReactiveUI;

namespace Asv.Drones.Gui.Core;

public class PlaningMissionUnknownPointViewModel : PlaningMissionPointViewModel
{
    public PlaningMissionUnknownPointViewModel(PlaningMissionPointModel point, PlaningMissionViewModel mission, IPlaningMission svc, ILocalizationService loc) : base(point, mission)
    {
        MissionAnchor = new PlaningMissionUnknownPointAnchor(point);
        
        this.WhenAnyValue(_ => _.Index)
            .Subscribe(_ =>
            {
                MissionAnchor.Index = _;
                IsChanged = true;
            }).DisposeItWith(Disposable);
        
        MissionAnchor.WhenAnyValue(_ => _.Location)
            .Subscribe(_ =>
            {
                Point.Location = _;
                IsChanged = true;
            }).DisposeItWith(Disposable);
    }
    
    public override void CreateVehicleItems(IVehicleClient vehicle, ISdrClientDevice? sdr)
    {
        //vehicle.Missions.AddTakeOffMissionItem(Point.Location);
    }
}
