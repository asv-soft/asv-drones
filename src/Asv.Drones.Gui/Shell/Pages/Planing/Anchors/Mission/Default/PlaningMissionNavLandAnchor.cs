using Asv.Drones.Gui.Api;
using Material.Icons;

namespace Asv.Drones.Gui;

public class PlaningMissionNavLandAnchor : PlaningMissionAnchor
{
    public PlaningMissionNavLandAnchor(PlaningMissionPointModel point) : base(point)
    {
        Icon = MaterialIconKind.FlightLand;
    }
}