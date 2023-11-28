using Asv.Common;
using Asv.Mavlink;
using Asv.Mavlink.V2.Common;

namespace Asv.Drones.Gui.Core;

public static class MissionItemHelper
{
    /// <summary>
    /// Adds navigation waypoint to MissionClients collection
    /// </summary>
    /// <param name="vehicle">vehicle client</param>
    /// <param name="point">item location</param>
    /// <param name="holdTime">time in seconds</param>
    /// <param name="acceptRadius">radius in meters</param>
    /// <param name="passRadius">radius in meters</param>
    /// <param name="yawAngle">angle in degrees</param>
    /// <returns>MissionItem creation result</returns>
    public static MissionItem AddNavMissionItem(this IMissionClientEx vehicle, GeoPoint point, 
        float holdTime = 0, float acceptRadius = 0, float passRadius = 0, float yawAngle = float.NaN)
    {
        MissionItem missionItem = vehicle.Create();
        missionItem.Location.OnNext(point);
        missionItem.AutoContinue.OnNext(true);
        missionItem.Command.OnNext(MavCmd.MavCmdNavWaypoint);
        missionItem.Current.OnNext(false);
        missionItem.Frame.OnNext(MavFrame.MavFrameGlobalInt);
        missionItem.MissionType.OnNext(MavMissionType.MavMissionTypeMission);
        missionItem.Param1.OnNext(holdTime);
        missionItem.Param2.OnNext(acceptRadius);
        missionItem.Param3.OnNext(passRadius);
        missionItem.Param4.OnNext(yawAngle);
        return missionItem;
    }
    
    public static MissionItem AddTakeOffMissionItem(this IMissionClientEx vehicle, GeoPoint point, float pitch = 0, float yawAngle = float.NaN)
    {
        MissionItem missionItem = vehicle.Create();
        missionItem.Location.OnNext(point);
        missionItem.AutoContinue.OnNext(true);
        missionItem.Command.OnNext(MavCmd.MavCmdNavTakeoff);
        missionItem.Current.OnNext(false);
        missionItem.Frame.OnNext(MavFrame.MavFrameGlobalInt);
        missionItem.MissionType.OnNext(MavMissionType.MavMissionTypeMission);
        missionItem.Param1.OnNext(pitch);
        missionItem.Param2.OnNext(0.0f);
        missionItem.Param3.OnNext(0.0f);
        missionItem.Param4.OnNext(yawAngle);
        return missionItem;
    }

    public static MissionItem AddLandMissionItem(this IMissionClientEx vehicle, GeoPoint point, float abortAltitude = 0,
        PrecisionLandMode landMode = PrecisionLandMode.PrecisionLandModeDisabled, float yawAngle = float.NaN)
    {
        MissionItem missionItem = vehicle.Create();
        missionItem.Location.OnNext(point);
        missionItem.AutoContinue.OnNext(true);
        missionItem.Command.OnNext(MavCmd.MavCmdNavLand);
        missionItem.Current.OnNext(false);
        missionItem.Frame.OnNext(MavFrame.MavFrameGlobalInt);
        missionItem.MissionType.OnNext(MavMissionType.MavMissionTypeMission);
        missionItem.Param1.OnNext(abortAltitude);
        missionItem.Param2.OnNext((float)landMode);
        missionItem.Param3.OnNext(0.0f);
        missionItem.Param4.OnNext(yawAngle);
        return missionItem;
    }
    
    public static MissionItem AddRoiMissionItem(this IMissionClientEx vehicle, GeoPoint point, MavRoi roiMode = MavRoi.MavRoiLocation, float wpIndex = 0, float roiIndex = 0)
    {
        MissionItem missionItem = vehicle.Create();
        missionItem.Location.OnNext(point);
        missionItem.AutoContinue.OnNext(true);
        missionItem.Command.OnNext(MavCmd.MavCmdDoSetRoi);
        missionItem.Current.OnNext(false);
        missionItem.Frame.OnNext(MavFrame.MavFrameGlobalInt);
        missionItem.MissionType.OnNext(MavMissionType.MavMissionTypeMission);
        missionItem.Param1.OnNext((float)roiMode);
        missionItem.Param2.OnNext(wpIndex);
        missionItem.Param3.OnNext(roiIndex);
        missionItem.Param4.OnNext(0.0f);
        return missionItem;
    }
    
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
            MavCmd.MavCmdNavWaypoint => PlaningMissionPointType.Navigation,
            MavCmd.MavCmdDoSetRoi or MavCmd.MavCmdDoSetRoiLocation => PlaningMissionPointType.Roi,
            _ => throw new ArgumentOutOfRangeException(command.ToString())
        };
    }  
}