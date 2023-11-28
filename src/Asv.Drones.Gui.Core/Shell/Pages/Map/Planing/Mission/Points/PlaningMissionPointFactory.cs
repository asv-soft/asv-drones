using System.ComponentModel.Composition;
using Material.Icons;

namespace Asv.Drones.Gui.Core;

public interface IPlaningMissionPointFactory
{
    public PlaningMissionPointViewModel Create(PlaningMissionPointModel point, PlaningMissionViewModel mission);
}

[Export(typeof(IPlaningMissionPointFactory))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class PlaningMissionPointFactory : IPlaningMissionPointFactory
{
    private readonly IPlaningMission _svc;
    private readonly ILocalizationService _loc;

    [ImportingConstructor]
    public PlaningMissionPointFactory(IPlaningMission svc, ILocalizationService loc)
    {
        _svc = svc;
        _loc = loc;
    }

    public PlaningMissionPointViewModel Create(PlaningMissionPointModel point, PlaningMissionViewModel mission)
    {
        switch (point.Type)
        {
            case PlaningMissionPointType.TakeOff:
                return new PlaningMissionTakeOffPointViewModel(point, mission, _svc, _loc);
            case PlaningMissionPointType.DoLand:
                return new PlaningMissionLandPointViewModel(point, mission, _svc, _loc);
            case PlaningMissionPointType.Waypoint:
                return new PlaningMissionNavigationPointViewModel(point, mission, _svc, _loc);
            case PlaningMissionPointType.Roi:
                return new PlaningMissionRoiPointViewModel(point, mission, _svc, _loc);
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}