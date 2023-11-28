using Asv.Avalonia.Map;
using Asv.Common;
using Asv.Mavlink;
using Asv.Mavlink.V2.Common;
using Material.Icons;
using ReactiveUI;

namespace Asv.Drones.Gui.Core;

public class PlaningMissionRoiPointViewModel : PlaningMissionPointViewModel
{
    public PlaningMissionRoiPointViewModel(PlaningMissionPointModel point, PlaningMissionViewModel mission, 
        IPlaningMission svc, ILocalizationService loc) : base(point, mission)
    {
        MissionAnchor = new PlaningMissionRoiPointAnchor(point);
        
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
        MissionClientExHelper.AddRoiMissionItem(vehicle.Missions, Point.Location);
        vehicle.Missions.AddRoiMissionItem(Point.Location, MavRoi.MavRoiLocation);
    }
}