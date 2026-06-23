using Asv.Avalonia.GeoMap;
using Asv.Common;
using Asv.IO;
using Asv.Mavlink;
using Asv.Mavlink.Common;

namespace Asv.Drones.Api;

public interface IMissionAnchor : IMapAnchor
{
    DeviceId DeviceId { get; }

    ushort MissionIndex { get; }

    MissionItem MissionItem { get; }
}
