using System.Collections.Immutable;
using Asv.IO;
using Asv.Mavlink;

namespace Asv.Drones.Api;

public interface IMavlinkMessagesExtension
{
    void Extend(ImmutableDictionary<int, Func<MavlinkMessage>>.Builder builder);
}

public interface IMavlinkHost
{
    IProtocolMessageFactory<MavlinkMessage, int> MessageFactory { get; }
    IMavlinkContext Context { get; }
    MavlinkIdentity Identity { get; }
    IHeartbeatServer? Heartbeat { get; }
}
