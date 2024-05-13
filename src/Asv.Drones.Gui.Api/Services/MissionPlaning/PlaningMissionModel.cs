using Asv.Common;
using Asv.Mavlink.V2.Common;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui.Api;

public record PlaningMissionModel
{
    public List<PlaningMissionPointModel> Points { get; set; } = new();
}

public class PlaningMissionPointModel
{
    /// <summary>
    /// Item index
    /// </summary>
    [Reactive]
    public int Index { get; set; }

    /// <summary>
    /// Command type
    /// </summary>
    [Reactive]
    public MavCmd Type { get; set; }

    /// <summary>
    /// Location
    /// </summary>
    [Reactive]
    public GeoPoint Location { get; set; }

    /// <summary>
    /// PARAM1, see MAV_CMD enum
    /// OriginName: param1, Units: , IsExtended: false
    /// </summary>
    [Reactive]
    public float Param1 { get; set; }

    /// <summary>
    /// PARAM2, see MAV_CMD enum
    /// OriginName: param2, Units: , IsExtended: false
    /// </summary>
    [Reactive]
    public float Param2 { get; set; }

    /// <summary>
    /// PARAM3, see MAV_CMD enum
    /// OriginName: param3, Units: , IsExtended: false
    /// </summary>
    [Reactive]
    public float Param3 { get; set; }

    /// <summary>
    /// PARAM4, see MAV_CMD enum
    /// OriginName: param4, Units: , IsExtended: false
    /// </summary>
    [Reactive]
    public float Param4 { get; set; }
}