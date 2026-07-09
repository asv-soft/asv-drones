using Asv.IO;

namespace Asv.Drones.Api;

public interface IDeviceFlightWidget<out TDeviceContext>
    : IFlightWidget,
        IDeviceActionTarget<TDeviceContext>
    where TDeviceContext : class, IClientDevice { }
