using Material.Icons;

namespace Asv.Drones.Gui.Core;

public class PlaningMissionNavSplineWaypointAnchor : PlaningMissionAnchor
{
    public PlaningMissionNavSplineWaypointAnchor(PlaningMissionPointModel point) : base(point)
    {
        Icon = MaterialIconKind.Location;
    }
}