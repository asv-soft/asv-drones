using Asv.Common;
using Asv.Mavlink;
using Asv.Mavlink.V2.Common;

namespace Asv.Drones.Gui.Core;

public static class MissionItemHelper
{
    public static PlaningMissionPointModel TransformMissionItemToPointModel(this MissionItem missionItem)
    {
        return new PlaningMissionPointModel
        {
            Index = missionItem.Index,
            Type = missionItem.Command.Value,
            Location = missionItem.Location.Value
        };
    }
}