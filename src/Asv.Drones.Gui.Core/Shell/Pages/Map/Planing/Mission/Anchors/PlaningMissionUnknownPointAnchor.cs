using Material.Icons;

namespace Asv.Drones.Gui.Core;

public class PlaningMissionUnknownPointAnchor : PlaningMissionAnchor
{
    public PlaningMissionUnknownPointAnchor(PlaningMissionPointModel point) : base(point)
    {
        Icon = MaterialIconKind.QuestionMark;
    }
}