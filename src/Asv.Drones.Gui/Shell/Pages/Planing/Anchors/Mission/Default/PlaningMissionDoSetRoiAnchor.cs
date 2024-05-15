using Asv.Drones.Gui.Api;
using Material.Icons;

namespace Asv.Drones.Gui;

public class PlaningMissionDoSetRoiAnchor : PlaningMissionAnchor
{
    public PlaningMissionDoSetRoiAnchor(PlaningMissionPointModel point) : base(point)
    {
        Icon = MaterialIconKind.ImageFilterCenterFocus;
    }
}