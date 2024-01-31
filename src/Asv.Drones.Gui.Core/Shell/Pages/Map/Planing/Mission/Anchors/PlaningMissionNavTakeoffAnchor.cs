using Material.Icons;

namespace Asv.Drones.Gui.Core;

public class PlaningMissionNavTakeoffAnchor : PlaningMissionAnchor
{
    public PlaningMissionNavTakeoffAnchor(PlaningMissionPointModel point) : base(point)
    {
        Icon = MaterialIconKind.FlightTakeoff;
    }
}