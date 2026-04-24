using Asv.IO;

namespace Asv.Drones.Api;

public interface IClientDeviceWidgetFactory
{
    public IFlightWidget? CreateWidget(in IClientDevice device);
}
