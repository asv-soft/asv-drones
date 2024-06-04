using Asv.Drones.Gui.Api;
using Material.Icons;

namespace Asv.Drones.Gui;

public class PlaningMissionUnknownAnchor : PlaningMissionAnchor
{
    public PlaningMissionUnknownAnchor(PlaningMissionPointModel point) : base(point)
    {
        Icon = MaterialIconKind.QuestionMark;
    }
}