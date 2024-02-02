using Material.Icons;

namespace Asv.Drones.Gui.Core;

public class PlaningMissionNavWaypointAnchor : PlaningMissionAnchor
{
    public PlaningMissionNavWaypointAnchor(PlaningMissionPointModel point) : base(point)
    {
        Icon = MaterialIconKind.Location;
    }
}