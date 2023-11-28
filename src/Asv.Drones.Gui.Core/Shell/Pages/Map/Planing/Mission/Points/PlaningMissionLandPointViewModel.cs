using Asv.Common;
using Asv.Mavlink;
using Material.Icons;
using ReactiveUI;

namespace Asv.Drones.Gui.Core;

public class PlaningMissionLandPointViewModel : PlaningMissionPointViewModel
{
    public PlaningMissionLandPointViewModel(PlaningMissionPointModel point, PlaningMissionViewModel mission, 
        IPlaningMission svc, ILocalizationService loc) : base(point, mission)
    {
        MissionAnchor = new PlaningMissionDoLandPointAnchor(point);
        
        this.WhenAnyValue(_ => _.Index)
            .Subscribe(_ =>
            {
                MissionAnchor.Index = _;
            }).DisposeItWith(Disposable);
        
        MissionAnchor.WhenAnyValue(_ => _.Location)
            .Subscribe(_ =>
            {
                Point.Location = _;
            }).DisposeItWith(Disposable);
    }
    
    public override void CreateVehicleItems(IVehicleClient vehicle, ISdrClientDevice? sdr)
    {
        vehicle.Missions.AddLandMissionItem(Point.Location);
    }
}

