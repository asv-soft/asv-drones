using Asv.Drones.Gui.Api;
using Material.Icons;

namespace Asv.Drones.Gui;

public class PlaningMissionNavSplineWaypointAnchor : PlaningMissionAnchor
{
    public PlaningMissionNavSplineWaypointAnchor(PlaningMissionPointModel point) : base(point)
    {
        Icon = MaterialIconKind.Location;
    }
}