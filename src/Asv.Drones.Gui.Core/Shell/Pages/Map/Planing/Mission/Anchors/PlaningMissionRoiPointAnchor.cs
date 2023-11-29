using Material.Icons;

namespace Asv.Drones.Gui.Core;

public class PlaningMissionRoiPointAnchor : PlaningMissionAnchor
{
    public PlaningMissionRoiPointAnchor(PlaningMissionPointModel point) : base(point)
    {
        Icon = MaterialIconKind.ImageFilterCenterFocus;
    }
}