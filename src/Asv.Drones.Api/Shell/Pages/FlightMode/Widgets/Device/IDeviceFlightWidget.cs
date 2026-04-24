using Asv.IO;

namespace Asv.Drones.Api;

public interface IDeviceFlightWidget<TDeviceContext> : IFlightWidget<TDeviceContext>
    where TDeviceContext : class, IClientDevice
{
    public TDeviceContext? Device { get; }
}
