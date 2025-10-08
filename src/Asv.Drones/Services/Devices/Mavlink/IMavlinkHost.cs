using Asv.Mavlink;

namespace Asv.Drones;

public interface IMavlinkHost
{
    MavlinkIdentity Identity { get; }
    IHeartbeatServer? Heartbeat { get; }
}