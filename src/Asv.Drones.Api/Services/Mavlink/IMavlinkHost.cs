using Asv.Mavlink;

namespace Asv.Drones.Api;

public interface IMavlinkHost
{
    MavlinkIdentity Identity { get; }
    IHeartbeatServer? Heartbeat { get; }
}