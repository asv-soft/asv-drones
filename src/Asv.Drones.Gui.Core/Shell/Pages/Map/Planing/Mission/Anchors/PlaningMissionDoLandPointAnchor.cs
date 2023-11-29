using Material.Icons;

namespace Asv.Drones.Gui.Core;

public class PlaningMissionDoLandPointAnchor : PlaningMissionAnchor
{
    public PlaningMissionDoLandPointAnchor(PlaningMissionPointModel point) : base(point)
    {
        Icon = MaterialIconKind.FlightLand;
    }
}