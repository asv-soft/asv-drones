using Material.Icons;

namespace Asv.Drones.Gui.Core;

public class PlaningMissionNavLandAnchor : PlaningMissionAnchor
{
    public PlaningMissionNavLandAnchor(PlaningMissionPointModel point) : base(point)
    {
        Icon = MaterialIconKind.FlightLand;
    }
}