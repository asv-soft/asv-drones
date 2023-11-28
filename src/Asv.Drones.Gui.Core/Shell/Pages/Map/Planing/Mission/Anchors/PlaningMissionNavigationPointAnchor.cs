using Material.Icons;

namespace Asv.Drones.Gui.Core;

public class PlaningMissionNavigationPointAnchor : PlaningMissionAnchor
{
    public PlaningMissionNavigationPointAnchor(PlaningMissionPointModel point) : base(point)
    {
        Icon = MaterialIconKind.Location;
    }
}