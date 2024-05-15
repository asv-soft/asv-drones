using Asv.Drones.Gui.Api;
using Material.Icons;

namespace Asv.Drones.Gui;

public class PlaningMissionNavWaypointAnchor : PlaningMissionAnchor
{
    public PlaningMissionNavWaypointAnchor(PlaningMissionPointModel point) : base(point)
    {
        Icon = MaterialIconKind.Location;
    }
}