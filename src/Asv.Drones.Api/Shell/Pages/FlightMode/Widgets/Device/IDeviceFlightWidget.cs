using Asv.IO;

namespace Asv.Drones.Api;

public interface IDeviceFlightWidget<out TDeviceContext> : IFlightWidget
    where TDeviceContext : class, IClientDevice
{
    public TDeviceContext Device { get; }
}
