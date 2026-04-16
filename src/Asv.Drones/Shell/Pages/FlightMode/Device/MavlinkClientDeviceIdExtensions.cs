using System;
using Asv.IO;
using Asv.Mavlink;

namespace Asv.Drones;

public static class MavlinkClientDeviceIdExtensions
{
    extension(DeviceId deviceId)
    {
        public int DisplayPriority =>
            deviceId switch
            {
                MavlinkClientDeviceId { DeviceClass: var dc } => dc.ToLowerInvariant() switch
                {
                    "copter" => 0,
                    "plane" => 1,
                    "vtol" => 2,
                    "gbs" => 10,
                    "sdr" => 20,
                    _ => int.MaxValue,
                },
                _ => int.MaxValue,
            };

        public int CompareWithinProtocol(DeviceId other) =>
            (deviceId, other) switch
            {
                (MavlinkClientDeviceId mx, MavlinkClientDeviceId my) => mx.Id.Target.SystemId
                != my.Id.Target.SystemId
                    ? mx.Id.Target.SystemId.CompareTo(my.Id.Target.SystemId)
                    : mx.Id.Target.ComponentId.CompareTo(my.Id.Target.ComponentId),
                _ => string.Compare(
                    deviceId.AsString(),
                    other.AsString(),
                    StringComparison.Ordinal
                ),
            };
    }
}
