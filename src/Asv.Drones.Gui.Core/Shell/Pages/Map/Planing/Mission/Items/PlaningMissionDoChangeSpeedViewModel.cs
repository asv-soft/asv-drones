using Asv.Common;
using Asv.Mavlink.V2.Common;
using Material.Icons;
using ReactiveUI;

namespace Asv.Drones.Gui.Core;

public class PlaningMissionDoChangeSpeedViewModel : PlaningMissionPointViewModel
{
    public PlaningMissionDoChangeSpeedViewModel(PlaningMissionPointModel point, PlaningMissionViewModel mission, 
        IPlaningMission svc, ILocalizationService loc) : base(point, mission)
    {
        Icon = MaterialIconKind.Speedometer;
        
        Param1Title = RS.PlaningMissionDoChangeSpeedViewModel_Param1Title;
        Param2Title = RS.PlaningMissionDoChangeSpeedViewModel_Param2Title;
        Param3Title = RS.PlaningMissionDoChangeSpeedViewModel_Param3Title;
        Param4Title = RS.PlaningMissionDoChangeSpeedViewModel_Param4Title;
    }
}