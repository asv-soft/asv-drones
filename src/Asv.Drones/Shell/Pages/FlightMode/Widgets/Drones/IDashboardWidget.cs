using Asv.Avalonia;
using Asv.IO;

namespace Asv.Drones;

public interface IDashboardWidget : IRoutable
{
    DeviceId DeviceId { get; }
    void Attach(DeviceId deviceId);
}
