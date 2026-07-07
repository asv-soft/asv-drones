using Asv.Avalonia.GeoMap;
using Asv.IO;
using Asv.Mavlink;

namespace Asv.Drones.Api;

public interface IMissionAnchor : IMapAnchor
{
    DeviceId DeviceId { get; }

    ushort MissionIndex { get; }

    MissionItem MissionItem { get; }
}
