using Asv.Mavlink.V2.Common;

namespace Asv.Drones.Gui.Core;

public static class PlaningMissionPointHelper
{
    public static string GetPlaningMissionPointName(this MavCmd type) => type.ToString().Replace(nameof(MavCmd), string.Empty);
}