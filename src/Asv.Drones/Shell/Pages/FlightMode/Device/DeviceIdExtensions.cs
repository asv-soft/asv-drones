using Asv.IO;
using Asv.Mavlink;

namespace Asv.Drones;

public static class DeviceIdExtensions
{
    extension(DeviceId deviceId)
    {
        public string Protocol =>
            deviceId switch
            {
                MavlinkClientDeviceId => "mavlink",
                _ => "unknown",
            };
    }
}
