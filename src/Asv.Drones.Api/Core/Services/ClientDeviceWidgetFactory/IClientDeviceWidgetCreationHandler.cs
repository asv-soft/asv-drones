using Asv.IO;

namespace Asv.Drones.Api;

public interface IClientDeviceWidgetCreationHandler
{
    Type DeviceType { get; }
    IFlightWidget? Create(in IClientDevice device);
}
