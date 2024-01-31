using Asv.Avalonia.Map;
using Asv.Common;
using Asv.Mavlink;
using Asv.Mavlink.V2.Common;
using Material.Icons;
using ReactiveUI;

namespace Asv.Drones.Gui.Core;

public class PlaningMissionNavTakeoffViewModel : PlaningMissionPointViewModel
{
    public PlaningMissionNavTakeoffViewModel(PlaningMissionPointModel point, PlaningMissionViewModel mission, 
        IPlaningMission svc, ILocalizationService loc) : base(point, mission)
    {
        MissionAnchor = new PlaningMissionNavTakeoffAnchor(point);
        
        Icon = MaterialIconKind.FlightTakeoff;
        
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
        
        Param1Title = RS.PlaningMissionNavTakeoffViewModel_Param1Title;
        Param2Title = RS.PlaningMissionNavTakeoffViewModel_Param2Title;
        Param3Title = RS.PlaningMissionNavTakeoffViewModel_Param3Title;
        Param4Title = RS.PlaningMissionNavTakeoffViewModel_Param4Title;
    }
}