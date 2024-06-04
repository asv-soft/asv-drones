using Asv.Drones.Gui.Api;
using Asv.Mavlink;
using Asv.Mavlink.V2.Common;

namespace Asv.Drones.Gui;

public static class MissionItemHelper
{
    public static PlaningMissionPointModel TransformMissionItemToPointModel(this MissionItem missionItem)
    {
        return new PlaningMissionPointModel
        {
            Index = missionItem.Index,
            Type = missionItem.Command.Value,
            Location = missionItem.Location.Value,
            Param1 = missionItem.Param1.Value,
            Param2 = missionItem.Param2.Value,
            Param3 = missionItem.Param3.Value,
            Param4 = missionItem.Param4.Value
        };
    }

    public static string GetPlaningMissionPointName(this MavCmd type) =>
        type.ToString().Replace(nameof(MavCmd), string.Empty);
}