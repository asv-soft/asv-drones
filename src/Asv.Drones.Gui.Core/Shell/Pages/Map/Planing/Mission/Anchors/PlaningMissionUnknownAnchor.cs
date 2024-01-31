using Material.Icons;

namespace Asv.Drones.Gui.Core;

public class PlaningMissionUnknownAnchor : PlaningMissionAnchor
{
    public PlaningMissionUnknownAnchor(PlaningMissionPointModel point) : base(point)
    {
        Icon = MaterialIconKind.QuestionMark;
    }
}