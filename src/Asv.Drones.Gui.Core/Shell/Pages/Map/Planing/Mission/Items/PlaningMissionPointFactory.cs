using System.ComponentModel.Composition;
using Asv.Common;
using Asv.Mavlink;
using Asv.Mavlink.V2.Common;
using Material.Icons;
using ReactiveUI;

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
            case MavCmd.MavCmdNavTakeoff:
                return new PlaningMissionNavTakeoffViewModel(point, mission, _svc, _loc);
            case MavCmd.MavCmdNavLand:
                return new PlaningMissionNavLandViewModel(point, mission, _svc, _loc);
            case MavCmd.MavCmdNavWaypoint:
                return new PlaningMissionNavWaypointViewModel(point, mission, _svc, _loc);
            case MavCmd.MavCmdDoSetRoi:
                return new PlaningMissionDoSetRoiViewModel(point, mission, _svc, _loc);
            case MavCmd.MavCmdDoChangeSpeed:
                return new PlaningMissionDoChangeSpeedViewModel(point, mission, _svc, _loc);
            case MavCmd.MavCmdNavSplineWaypoint:
                return new PlaningMissionNavSplineWaypointViewModel(point, mission, _svc, _loc);
            default:
                return new PlaningMissionUnknownViewModel(point, mission, _svc, _loc);
        }
    }
}