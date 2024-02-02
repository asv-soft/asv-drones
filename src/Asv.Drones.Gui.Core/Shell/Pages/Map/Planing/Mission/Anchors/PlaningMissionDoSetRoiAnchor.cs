using Material.Icons;

namespace Asv.Drones.Gui.Core;

public class PlaningMissionDoSetRoiAnchor : PlaningMissionAnchor
{
    public PlaningMissionDoSetRoiAnchor(PlaningMissionPointModel point) : base(point)
    {
        Icon = MaterialIconKind.ImageFilterCenterFocus;
    }
}