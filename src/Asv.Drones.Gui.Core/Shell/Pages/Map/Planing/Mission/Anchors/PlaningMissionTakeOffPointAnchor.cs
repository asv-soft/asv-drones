using Material.Icons;

namespace Asv.Drones.Gui.Core;

public class PlaningMissionTakeOffPointAnchor : PlaningMissionAnchor
{
    public PlaningMissionTakeOffPointAnchor(PlaningMissionPointModel point) : base(point)
    {
        Icon = MaterialIconKind.FlightTakeoff;
    }
}