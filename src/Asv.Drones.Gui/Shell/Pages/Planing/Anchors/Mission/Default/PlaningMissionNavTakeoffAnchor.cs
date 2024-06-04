using Asv.Drones.Gui.Api;
using Material.Icons;

namespace Asv.Drones.Gui;

public class PlaningMissionNavTakeoffAnchor : PlaningMissionAnchor
{
    public PlaningMissionNavTakeoffAnchor(PlaningMissionPointModel point) : base(point)
    {
        Icon = MaterialIconKind.FlightTakeoff;
    }
}