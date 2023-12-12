using Asv.Common;
using Asv.Mavlink;
using Asv.Mavlink.V2.Common;

namespace Asv.Drones.Gui.Core;

public static class MissionItemHelper
{
    public static PlaningMissionPointModel TransformMissionItemToPointModel(this MissionItem missionItem)
    {
        var pointType = GetMissionPointTypeFromMavlinkCommand(missionItem.Command.Value);
        return new PlaningMissionPointModel
        {
            Index = missionItem.Index,
            Type = pointType,
            Location = missionItem.Location.Value
        };
    }

    public static PlaningMissionPointType GetMissionPointTypeFromMavlinkCommand(MavCmd command)
    {
        return command switch
        {
            MavCmd.MavCmdNavLand => PlaningMissionPointType.DoLand,
            MavCmd.MavCmdNavTakeoff => PlaningMissionPointType.TakeOff,
            MavCmd.MavCmdNavWaypoint => PlaningMissionPointType.Waypoint,
            MavCmd.MavCmdDoSetRoi or MavCmd.MavCmdDoSetRoiLocation => PlaningMissionPointType.Roi,
            _ => throw new ArgumentOutOfRangeException(command.ToString())
        };
    }  
}