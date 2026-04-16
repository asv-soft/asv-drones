using Asv.IO;

namespace Asv.Drones;

public interface IFlightModeOrderable
{
    DeviceId? DeviceId { get; }
    int DisplayPriority { get; }
    int SubOrder { get; }
}
